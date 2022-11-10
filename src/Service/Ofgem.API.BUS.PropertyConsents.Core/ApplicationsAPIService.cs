using Ofgem.API.BUS.Applications.Client.Interfaces;
using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.Applications.Domain.Constants;
using Ofgem.API.BUS.Applications.Domain.Entities.CommsObjects;
using Ofgem.API.BUS.PropertyConsents.Core.Interfaces;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using Ofgem.Lib.BUS.APIClient.Domain.Exceptions;
using Ofgem.Lib.BUS.APIClient.Domain.Models;
using static Ofgem.API.BUS.Applications.Domain.ApplicationSubStatus;
using static Ofgem.API.BUS.Applications.Domain.Entities.AuditLog;

namespace Ofgem.API.BUS.PropertyConsents.Core;

public class ApplicationsAPIService : IApplicationsAPIService
{
    private readonly IApplicationsAPIClient _applicationsAPIClient;
    private readonly IOwnerConsentService _ownerConsentService;

    public ApplicationsAPIService(IApplicationsAPIClient applicationsAPIClient, IOwnerConsentService ownerConsentService)
    {
        _applicationsAPIClient = applicationsAPIClient ?? throw new ArgumentNullException(nameof(applicationsAPIClient));
        _ownerConsentService = ownerConsentService ?? throw new ArgumentNullException(nameof(ownerConsentService));
    }

    public async Task<ConsentRequestSummary> GetConsentRequestSummaryAsync(Guid consentRequestId)
    {
        GetConsentRequestDetailsResult getConsentRequestDetailsResult = await _applicationsAPIClient.ConsentRequestsRequestsClient.GetDetailsAsync(consentRequestId);

        if (getConsentRequestDetailsResult.IsSuccess == false)
        {
            throw new InvalidOperationException("API call succeeded, but returned a non-success result");
        }

        if (getConsentRequestDetailsResult.ConsentRequestSummary == null)
        {
            throw new InvalidOperationException("API call succeeded, but ConsentRequestSummary was null");
        }

        return getConsentRequestDetailsResult.ConsentRequestSummary;
    }

    public async Task<string> GetBusinessAccountEmailByInstallerId(Guid installerId)
    {
        string email = await _applicationsAPIClient.ApplicationsRequestsClient.GetBusinessAccountEmailByInstallerId(installerId);
        return email;
    }

    public async Task<RegisterOwnerConsentResult> RegisterConsentAsync(Guid consentRequestId)
    {
        var associatedApplications = await GetAssociatedApplications(consentRequestId);
        var isEligible = IsApplicationEligible(associatedApplications);
        var returnResult = new RegisterOwnerConsentResult { IsIneligible = false, IsSuccess = false };

        if (isEligible)
        {
            var currentApplication = associatedApplications.First(app => app.ConsentRequests.Any(con => con.ID == consentRequestId));
            var updateUser = currentApplication.PropertyOwnerDetail?.Email ?? "Unknown User";
            var auditLogParameters = new AuditLogParameters
            {
                EntityReferenceId = currentApplication.ID,
                Username = updateUser,
                UserType = AuditLogUserType.Consent.ToString()
            };

            var registerConsentReceivedRequest = new RegisterConsentReceivedRequest { UpdatedByUsername = updateUser };

            await _applicationsAPIClient.ConsentRequestsRequestsClient.RegisterConsentReceivedAsync(consentRequestId, registerConsentReceivedRequest, auditLogParameters);

            var isUpdateStatuses = await HandleCompetingApplications(associatedApplications, auditLogParameters);
            await RejectLosingApplications(associatedApplications, consentRequestId);
            returnResult.IsSuccess = isUpdateStatuses;
        }
        else
        {
            returnResult.IsIneligible = true;
        }

        return returnResult;
    }

    public async Task<IEnumerable<Application>> GetAssociatedApplications(Guid consentRequestId)
    {
        var consentRequest = await GetConsentRequestSummaryAsync(consentRequestId);
        var associatedApplications = new List<Application>();

        if (consentRequest == null)
        {
            throw new ResourceNotFoundException($"Consent request {consentRequestId} could not be found");
        }

        if (string.IsNullOrEmpty(consentRequest.InstallationAddressUprn))
        {
            var currentApplication = await _applicationsAPIClient.ApplicationsRequestsClient.GetApplicationByReferenceNumberAsync(consentRequest.ApplicationReferenceNumber) ??
                throw new ResourceNotFoundException($"Could not find an application for reference number {consentRequest.ApplicationReferenceNumber}");

            associatedApplications.Add(currentApplication);
        }
        else
        {
            var applicationsByUprn = await GetApplicationsByUprnAsync(consentRequest.InstallationAddressUprn);
            associatedApplications = applicationsByUprn.ToList();
        }
        
        return associatedApplications;
    }

    public async Task<IEnumerable<Application>> GetApplicationsByUprnAsync(string uprn)
    {
        return await _applicationsAPIClient.ApplicationsRequestsClient.GetApplicationsByUprnAsync(uprn);
    }

    public bool IsApplicationEligible(IEnumerable<Application> applications)
    {
        if (applications == null || !applications.Any())
        {
            throw new ArgumentNullException(nameof(applications));
        }

        // Applications with a valid consent have a valid consent received date and are not in any of the rejected statuses.
        var isUprnConsented = applications.Any(app => (app.ConsentRequests != null && app.ConsentRequests.Any(cons => cons.ConsentReceivedDate != null))
                                                     && (app.SubStatus != null && (app.SubStatus.Code != ApplicationSubStatusCode.CNTRD
                                                                                               && app.SubStatus.Code != ApplicationSubStatusCode.VEXPD
                                                                                               && app.SubStatus.Code != ApplicationSubStatusCode.REJECTED
                                                                                               && app.SubStatus.Code != ApplicationSubStatusCode.WITHDRAWN
                                                                                               && app.SubStatus.Code != ApplicationSubStatusCode.RPEND)));

        return !isUprnConsented;
    }

    /// <summary>
    /// Handles losing applications - sends an email to the installer.
    /// </summary>
    /// <param name="applications">A list of applications with their consent requests included.</param>
    /// <param name="winningConsentRequestId">The ID of the consent request which has received consent.</param>
    public async Task RejectLosingApplications(IEnumerable<Application> applications, Guid winningConsentRequestId)
    {
        if (applications.Any())
        {
            var applicationsWithConsent = applications.Where(app => app.ConsentRequests != null
                                                                    && app.ConsentRequests.Any(f => f.ID.Equals(winningConsentRequestId)));

            var losingApplications = applications.Where(v => v.SubStatusId != StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTPS])
                                                 .Except(applicationsWithConsent)
                                                 .ToList();

            if (losingApplications.Any())
            {
                foreach (var application in losingApplications)
                {
                    await SendOwnerConsentedElsewhereEmail(application.ConsentRequests.First().ID);
                }
            }
        }
    }

    /// <summary>
    /// Updates the application status where competing applications exists. The application status for the winning application
    /// and losing application is set to Consent Review. If one application is provided, nothing is updated.
    /// </summary>
    /// <param name="applications">The applications to update. Includes the winning application.</param>
    /// <param name="winningConsentRequestId"></param>
    /// <param name="auditLogParams">Audit log params</param>
    /// <returns>True if the operation succeeded or was unnecessary, false if there was a problem</returns>
    public async Task<bool> HandleCompetingApplications(IEnumerable<Application> applications, AuditLogParameters auditLogParams)
    {
        bool isSuccess = true;

        if (applications.Any() && applications.Count() > 1)
        {
            var applicationsToUpdate = applications.Where(v => v.SubStatusId != StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTPS])
                                                   .ToList();

            if (applicationsToUpdate.Any())
            {
                var updateStatusErrors = new List<string>();

                foreach (var applicationId in applicationsToUpdate.Select(application => application.ID))
                {
                    auditLogParams.EntityReferenceId = applicationId;
                    var statusCode = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.CNTRW];

                    var result = await _applicationsAPIClient.ApplicationsRequestsClient.UpdateApplicationStatusAsync(applicationId, statusCode, auditLogParams);

                    updateStatusErrors.AddRange(result);
                }

                isSuccess = !updateStatusErrors.Any();
            }
        }

        return isSuccess;
    }

    public async Task<bool> StoreFeedback(StoreFeedBackRequest feedback)
    {
        var isSuccess = false;
        
        if (feedback != null)
        {
            try
            {
                var associatedApplications = await GetAssociatedApplications(feedback.ConsentRequestId);

                var currentApplication = associatedApplications.First(app => app.ConsentRequests.Any(con => con.ID == feedback.ConsentRequestId));

                var auditParams = new AuditLogParameters
                {
                    EntityReferenceId = currentApplication.ID,
                    Username = currentApplication.PropertyOwnerDetail?.Email ?? "Unknown User",
                    UserType = AuditLogUserType.Consent.ToString()
                };

                var feedbackList = new Dictionary<string, string>()
                {
                    { "ApplicationId", currentApplication.ID.ToString() },
                    { "FeedbackNarrative" , feedback.FeedbackNarrative },
                    { "SurveyOption" , feedback.SurveyOption.ToString() },
                    { "ServiceUsed" , auditParams.UserType }
                };

                await _applicationsAPIClient.ApplicationsRequestsClient.StoreServiceFeedback(feedbackList, auditParams);

                isSuccess = true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        return isSuccess;
    }

    private async Task SendOwnerConsentedElsewhereEmail(Guid consentRequestId)
    {
        var applicationConsentRequestSummary = await GetConsentRequestSummaryAsync(consentRequestId);
        var emailRequest = new SendInstallersNotChosenEmailRequest
        {
            InstallationAddressLine1 = applicationConsentRequestSummary.InstallationAddressLine1,
            InstallationAddressLine2 = applicationConsentRequestSummary.InstallationAddressLine2,
            InstallationAddressLine3 = applicationConsentRequestSummary.InstallationAddressLine3,
            InstallationAddressCounty = applicationConsentRequestSummary.InstallationAddressCounty,
            InstallationAddressPostcode = applicationConsentRequestSummary.InstallationAddressPostcode,
            InstallerEmailAddress = applicationConsentRequestSummary.InstallerEmailId,
            TechnologyType = applicationConsentRequestSummary.TechnologyType
        };

        await _ownerConsentService.SendRejectionEmailToInstallersNotChosenAsync(emailRequest);
    }

}
