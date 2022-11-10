namespace Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects
{
    public record TokenVerificationResult
    {
        /// <summary>
        /// The outcome of token validation, with decoded claims
        /// </summary>
        public bool TokenAccepted { get; set; }
        public Guid? ConsentRequestId { get; set; }
        public DateTime? TokenExpiryDate { get; set; }
    }
}
