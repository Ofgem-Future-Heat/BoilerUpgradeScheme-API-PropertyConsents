using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using Ofgem.Lib.BUS.APIClient.Domain.Models;

namespace Ofgem.API.BUS.PropertyConsents.Core.Interfaces;

/// <summary>
/// Interface for functions which will consume the Ofgem.API.BUS.Applications API
/// </summary>
public  interface IApplicationsAPIService
{
    /// <summary>
    /// Informs the Applications API that consent has been received for a given ConsentRequestId
    /// </summary>
    /// <param name="consentRequestId">The consent request ID.</param>
    /// <returns>A <see cref="RegisterOwnerConsentResult"/> object indicating if consent has been updated or if the request is ineligible.</returns>
    Task<RegisterOwnerConsentResult> RegisterConsentAsync(Guid consentRequestId);

    /// <summary>
    /// Gets the application data to be shown to the property owner for consent for a given ConsentRequestId
    /// </summary>
    /// <param name="consentRequestId"></param>
    /// <returns></returns>
    public Task<ConsentRequestSummary> GetConsentRequestSummaryAsync(Guid consentRequestId);

    /// <summary>
    /// Gets the primary email address by Installers "External User" Id. 
    /// </summary>
    /// <param name="businessAccountId"></param>
    /// <returns></returns>
    public Task<string> GetBusinessAccountEmailByInstallerId(Guid installerId);

    /// <summary>
    /// Gets a list of applications associated with a UPRN.
    /// </summary>
    /// <param name="uprn">The UPRN to search from.</param>
    /// <returns>A list of <see cref="Application"/> objects.</returns>
    Task<IEnumerable<Application>> GetApplicationsByUprnAsync(string uprn);

    /// <summary>
    /// Gets a list of applications associated with the current consent request. Applications are linked by UPRN.
    /// </summary>
    /// <param name="consentRequestId">The consent request ID.</param>
    /// <returns>A list of <see cref="Application"/>s associated with the consent request.</returns>
    Task<IEnumerable<Application>> GetAssociatedApplications(Guid consentRequestId);

    /// <summary>
    /// Checks that an application is eligible to be consented by checking if other applications associated with an address
    /// have not received consent.
    /// </summary>
    /// <param name="applications">A list of applications associated with a consent request.</param>
    /// <returns><c>true</c> if the consent request is eligible to proceed.</returns>
    bool IsApplicationEligible(IEnumerable<Application> applications);

    /// <summary>
    /// Stores property owner feedback to the database
    /// </summary>
    /// <param name="feedback">Feedback object containing all relevant data.</param>
    /// <returns>boolean</returns>
    public Task<bool> StoreFeedback(StoreFeedBackRequest feedback);

    /// <summary>
    /// Updates the application status where competing applications exists. The application status for the winning application
    /// and losing application is set to Consent Review.
    /// </summary>
    /// <param name="applications">The applications to update. Includes the winning application.</param>
    /// <param name="winningConsentRequestId"></param>
    /// <param name="auditLogParams">Audit log params</param>
    /// <returns>True if the operation succeeded or was unnecessary, false if there was a problem</returns>
    Task<bool> HandleCompetingApplications(IEnumerable<Application> applications, AuditLogParameters auditLogParams);

    /// <summary>
    /// Handles losing applications - sends an email to the installer.
    /// </summary>
    /// <param name="applications">A list of applications with their consent requests included.</param>
    /// <param name="winningConsentRequestId">The ID of the consent request which has received consent.</param>
    Task RejectLosingApplications(IEnumerable<Application> applications, Guid winningConsentRequestId);
}
