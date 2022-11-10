namespace Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects
{
    /// <summary>
    /// Dto containing all data required to save the property owners feedback
    /// </summary>
    public class StoreFeedBackRequest
    {
        public Guid ConsentRequestId { get; set; }
        public int SurveyOption{ get; set; }
        public string FeedbackNarrative { get; set; } = null!;
    }
}
