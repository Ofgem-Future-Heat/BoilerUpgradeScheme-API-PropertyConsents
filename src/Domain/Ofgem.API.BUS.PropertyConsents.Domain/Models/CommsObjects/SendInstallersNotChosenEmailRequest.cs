namespace Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects
{
    /// <summary>
    /// Dto containing all data required to generate the Installer Not Chosen email
    /// </summary>
    public class SendInstallersNotChosenEmailRequest
    {
        public string InstallerEmailAddress { get; set; } = null!;
        public string InstallerName { get; set; } = null!;
        public string TechnologyType { get; set; } = null!;
        public string InstallationAddressLine1 { get; set; } = null!;
        public string InstallationAddressLine2 { get; set; } = null!;
        public string? InstallationAddressLine3 { get; set; }
        public string InstallationAddressCounty { get; set; } = null!;
        public string InstallationAddressPostcode { get; set; } = null!;
    }
}
