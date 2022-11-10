using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;

namespace Ofgem.API.BUS.PropertyConsents.Core.Interfaces;

/// <summary>
/// Interface for owner consent functions
/// </summary>
public interface IOwnerConsentService
{
    /// <summary>
    /// Attempts to send a property owner consent email with variables populated as per the Request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<SendConsentEmailResult> SendConsentEmailAsync(SendConsentEmailRequest request);
    /// <summary>
    /// Verifies a bearer token passed from a url request.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    TokenVerificationResult VerifyToken(string token);

    /// <summary>
    /// Attempts to send a property owner consent confirm email with variables populated 
    /// </summary>
    /// <param name="request"></param>
    /// <returns>SendConsentConfirmationEmailResult object</returns>
    Task<SendConsentConfirmationEmailResult> SendConsentConfirmationEmailAsync(SendConsentConfirmationEmailRequest request);

    /// <summary>
    /// Attempts to send a rejection/cancellation email to all applying installers that wasnt chosen by a property owner for a Boiler installation.
    /// </summary>
    /// <param name="installersNotChosen"></param>
    /// <returns></returns>
    Task<SendInstallersNotChosenEmailResult> SendRejectionEmailToInstallersNotChosenAsync(SendInstallersNotChosenEmailRequest request);
}
