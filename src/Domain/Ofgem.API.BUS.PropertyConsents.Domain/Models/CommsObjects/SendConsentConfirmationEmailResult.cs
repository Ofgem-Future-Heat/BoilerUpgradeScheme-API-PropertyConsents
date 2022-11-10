namespace Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects
{
    public record SendConsentConfirmationEmailResult
    {
        /// <summary>
        /// Result of an attempt to send the Consent email
        /// </summary>
        public bool IsSuccess { get; init; }
        public Guid ConsentRequestId { get; init; }
    }
}
