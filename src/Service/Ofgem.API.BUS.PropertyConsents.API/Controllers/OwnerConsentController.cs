using Microsoft.AspNetCore.Mvc;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using Ofgem.API.BUS.PropertyConsents.Core.Interfaces;
using Ofgem.API.BUS.PropertyConsents.API.Extensions;
using Ofgem.Lib.BUS.APIClient.Domain.Exceptions;

namespace Ofgem.API.BUS.PropertyConsents.API.Controllers
{
    /// <summary>
    /// This OwnerConsent controller routes all Owner Consent oprations to the OwnerConsent Service class.
    /// </summary>
    [ApiController]
    [Route("")]
    public class OwnerConsentController : ControllerBase
    {
        private readonly IOwnerConsentService _ownerConsentService;
        private readonly IApplicationsAPIService _applicationsAPIService;

        public OwnerConsentController(IOwnerConsentService ownerConsentService, IApplicationsAPIService applicationsService)
        {
            _ownerConsentService = ownerConsentService ?? throw new ArgumentNullException(nameof(ownerConsentService));
            _applicationsAPIService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));
        }

        /// <summary>
        /// Raises a new Owner Consent Request email. Expected to be called by the Applications API. Token not required for use (it makes the tokens)
        /// when a new application is submitted
        /// </summary>
        /// <param name="sendConsentEmailRequest"></param>
        /// <returns></returns>
        [HttpPost("")]
        [ProducesResponseType(typeof(SendConsentEmailResult), 200)]
        public async Task<IActionResult> SendConsentEmail(SendConsentEmailRequest sendConsentEmailRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var sendConsentEmailResult = await _ownerConsentService.SendConsentEmailAsync(sendConsentEmailRequest);

                return Ok(sendConsentEmailResult);
            }
            catch (BadRequestException ex)
            {
                return this.AsObjectResult(ex);
            }
        }


        /// <summary>
        /// Registers Owner Consent with the Application API
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("/{consentRequestId}")]
        [ProducesResponseType(typeof(RegisterOwnerConsentResult), 200)]
        public async Task<IActionResult> RegisterOwnerConsentReceived(Guid consentRequestId)
        {
            try
            {
                var returnResult = await _applicationsAPIService.RegisterConsentAsync(consentRequestId);

                return Ok(returnResult);
            }
            catch (BadRequestException ex)
            {
                return this.AsObjectResult(ex);
            }
        }

        /// <summary>
        /// Decrypts tokens used in Consent email
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("verify")]
        [ProducesResponseType(typeof(TokenVerificationResult), 200)]
        public IActionResult VerifyToken(string token)
        {
            try
            {
                var tokenResult = _ownerConsentService.VerifyToken(token);

                return Ok(tokenResult);
            }
            catch (BadRequestException ex)
            {
                return this.AsObjectResult(ex);
            }
        }

        /// <summary>
        /// Gets a summary of the application relating to a given consent ID
        /// </summary>
        /// <param name="consentRequestId"></param>
        /// <returns></returns>
        [HttpGet("/{consentRequestId}")]
        [ProducesResponseType(typeof(ConsentRequestSummary), 200)]
        public async Task<IActionResult> GetConsentRequestSummary(Guid consentRequestId)
        {
            try
            {
                var consentRequestSummary = await _applicationsAPIService.GetConsentRequestSummaryAsync(consentRequestId);

                return Ok(consentRequestSummary);
            }
            catch (BadRequestException ex)
            {
                return this.AsObjectResult(ex);
            }
        }

        /// <summary>
        /// Raises a new Owner Consent Confirm email and Installer consent confirm email.
        /// </summary>
        /// <param name="consentRequestId"></param>
        /// <returns></returns>
        [HttpPost("/OwnerAndInstaller/{consentRequestId}")]
        [ProducesResponseType(typeof(SendConsentConfirmationEmailResult), 200)]
        public async Task<IActionResult> SendConfirmationConsentEmail(Guid consentRequestId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var consentDetails = await _applicationsAPIService.GetConsentRequestSummaryAsync(consentRequestId);

                var sendConsentConfirmRequest = new SendConsentConfirmationEmailRequest
                {
                    ConsentRequestId = consentRequestId,
                    OwnerEmailAddress = consentDetails.OwnerEmailId,
                    InstallerEmailAddress = consentDetails.InstallerEmailId,
                    ApplicationReferenceNumber = consentDetails.ApplicationReferenceNumber,
                    InstallerName = consentDetails.InstallerName,
                    TechnologyType = consentDetails.TechnologyType,
                    InstallationAddressLine1 = consentDetails.InstallationAddressLine1,
                    InstallationAddressLine2 = consentDetails.InstallationAddressLine2,
                    InstallationAddressLine3 = consentDetails.InstallationAddressLine3,
                    InstallationAddressCounty = consentDetails.InstallationAddressCounty,
                    InstallationAddressPostcode = consentDetails.InstallationAddressPostcode
                };

                var sendConsentConfirmationEmailResult = await _ownerConsentService.SendConsentConfirmationEmailAsync(sendConsentConfirmRequest);
                return Ok(sendConsentConfirmationEmailResult);
            }
            catch (BadRequestException ex)
            {
                return this.AsObjectResult(ex);
            }
        }

        /// <summary>
        /// Sends rejection emails to installers who applied to install a heatpump where the home-owner has given consent to another installer
        /// </summary>
        /// <param name="consentRequestId"></param>
        /// <returns></returns>
        [HttpPost("/SendInstallerRejectionEmail/{consentRequestId}")]
        [ProducesResponseType(typeof(SendConsentConfirmationEmailResult), 200)]
        public async Task<IActionResult> SendApplicationRejectionEmailToinstallersNotChosen(Guid consentRequestId)
        {
            try
            {
                var applicationConsentRequestSummary = await _applicationsAPIService.GetConsentRequestSummaryAsync(consentRequestId);
                var installerChosen = applicationConsentRequestSummary.InstallerEmailId;

                var installersAssociatedApplication = await _applicationsAPIService.GetAssociatedApplications(consentRequestId);

                var installersOnApplication = installersAssociatedApplication.Select(bId => bId.SubmitterId).ToList();

                var businessAccountEmails = new List<string>();

                foreach (Guid id in installersOnApplication)
                {
                    var item = await _applicationsAPIService.GetBusinessAccountEmailByInstallerId(id);
                    businessAccountEmails.Add(item);
                }

                //remove {installerChosen} from the list of installers that will be rejected
                var installersNotChosen = businessAccountEmails.Where(email => !email.Equals(installerChosen)).ToList();


                foreach (string rejectedInstallerEmailAddress in installersNotChosen)
                {
                    var request = new SendInstallersNotChosenEmailRequest
                    {
                        InstallationAddressCounty = applicationConsentRequestSummary.InstallationAddressCounty,
                        InstallationAddressLine1 = applicationConsentRequestSummary.InstallationAddressLine1,
                        InstallationAddressLine2 = applicationConsentRequestSummary.InstallationAddressLine2,
                        InstallationAddressLine3 = applicationConsentRequestSummary.InstallationAddressLine3,
                        InstallationAddressPostcode = applicationConsentRequestSummary.InstallationAddressPostcode,
                        InstallerEmailAddress = rejectedInstallerEmailAddress,
                        TechnologyType = applicationConsentRequestSummary.TechnologyType
                    };

                    var sendConsentConfirmationEmailResult = await _ownerConsentService.SendRejectionEmailToInstallersNotChosenAsync(request);
                }

                var sendEmailToInstallersNotChosenResult = new SendInstallersNotChosenEmailResult
                {
                    IsSuccess = true
                };

                return Ok(sendEmailToInstallersNotChosenResult);
            }
            catch (BadRequestException ex)
            {
                return this.AsObjectResult(ex);
            }

        }

        /// <summary>
        /// Sends PO feedback to be stored in the application database.
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        [HttpPost("/StoreFeedback/")]
        [ProducesResponseType(typeof(StoreFeedBackResult), 200)]
        public async Task<IActionResult> StoreFeedback(StoreFeedBackRequest feedback)
        {
            try
            {
                await _applicationsAPIService.StoreFeedback(feedback);
            }

            catch (Exception ex)
            {
                return BadRequest(new StoreFeedBackResult { IsSuccess = false });
            }

            return Ok(new StoreFeedBackResult { IsSuccess = true });
        }
    }
}