namespace Ofgem.API.BUS.PropertyConsents.Core.Interfaces;

/// <summary>
/// Interface for configuration data needed by the OwnerConsentService class
/// </summary>
public interface IOwnerConsentServiceOptions
{
    /// <summary>
    /// The ID of the Gov.Notify template used to generate consent emails 
    /// </summary>
    public string ConsentEmailTemplateID { get; }

    /// <summary>
    /// The secret for generating and validating the consent email's 14-day token
    /// </summary>
    public string ConsentTokenSecret { get; }

    /// <summary>
    /// The ID of the Gov.Notify template used to generate consent confirm email to owner
    /// </summary>
    public string ConsentOwnerConfirmEmailTemplateID { get; }

    /// <summary>
    /// The ID of the Gov.Notify template used to generate a email informing installers that they have not been chosen
    /// </summary>
    public string ConsentInstallerNotChosenEmailTemplateID { get; }

    /// <summary>
    /// The ID of the Gov.Notify template used to generate consent confirm email to installer
    /// </summary>
    public string ConsentInstallerConfirmEmailTemplateID { get; }
}
