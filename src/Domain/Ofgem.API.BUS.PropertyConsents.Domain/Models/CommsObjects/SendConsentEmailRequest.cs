namespace Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects
{
    /// <summary>
    /// Dto containing all data required to generate the Property Owner email
    /// </summary>
    public record SendConsentEmailRequest
    {
        public Guid ConsentRequestId { get; set; }
        public int ConsentRequestExpiryDays { get; set; }
        public string EmailAddress { get; set; } = null!;
        public string ApplicationReferenceNumber { get; set; } = null!;
        public string InstallerName { get; set; } = null!;
        public string TechnologyType { get; set; } = null!;
        public string InstallationAddressLine1 { get; set; } = null!;
        public string InstallationAddressLine2 { get; set; } = null!;
        public string? InstallationAddressLine3 { get; set; }
        public string InstallationAddressCounty { get; set; } = null!;
        public string InstallationAddressPostcode { get; set; } = null!;
        public Uri PropertyOwnerPortalBaseURL { get; set; } = null!;
    }
}
