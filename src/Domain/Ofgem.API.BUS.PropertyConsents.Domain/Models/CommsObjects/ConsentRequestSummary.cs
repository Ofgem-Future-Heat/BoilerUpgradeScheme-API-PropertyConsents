namespace Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects
{
    /// <summary>
    /// Dto containing all data required to populate the Owner Consent page on the Consent Portal
    /// </summary>
    public record ConsentRequestSummary
    {
        public string ApplicationReferenceNumber { get; set; } = null!;
        public string InstallerName { get; set; } = null!;
        public string InstallerEmailId { get; set; } = null!;
        public string OwnerEmailId { get; set; } = null!;
        public string OwnerFullName { get; set; } = null!;
        public string TechnologyType { get; set; } = null!;
        public string InstallationAddressLine1 { get; set; } = null!;
        public string InstallationAddressLine2 { get; set; } = null!;
        public string? InstallationAddressLine3 { get; set; }
        public string InstallationAddressCounty { get; set; } = null!;
        public string InstallationAddressPostcode { get; set; } = null!;
        public string InstallationAddressUprn { get; set; } = null!;
        public DateTime ServiceLevelAgreementDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal QuoteAmount { get; set; }
        public DateTime? HasConsented { get; set; }
    }
}
