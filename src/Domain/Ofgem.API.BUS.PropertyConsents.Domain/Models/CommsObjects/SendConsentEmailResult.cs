namespace Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects
{
    public record SendConsentEmailResult
    {
        /// <summary>
        /// Result of an attempt to send the Consent email
        /// </summary>
        public bool IsSuccess { get; set; } 
        public Guid ConsentRequestId  { get; set; }
        public DateTime? ConsentTokenExpires { get; set; }
    }
}
