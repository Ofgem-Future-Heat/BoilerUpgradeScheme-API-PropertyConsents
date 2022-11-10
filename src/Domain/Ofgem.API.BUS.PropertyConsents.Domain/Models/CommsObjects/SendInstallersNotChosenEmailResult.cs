namespace Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects
{
    public class SendInstallersNotChosenEmailResult
    {
        /// <summary>
        /// Result of an attempt to send email to all installers not chosen during owner consent
        /// </summary>
        public bool IsSuccess { get; init; }
    }
}
