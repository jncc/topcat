namespace Catalogue.Web.Controllers.Records
{
    public class PublishingState
    {
        public bool AssessedAndUpToDate { get; set; }
        public bool SignedOffAndUpToDate { get; set; }
        public bool PublishedToHubAndUpToDate { get; set; }
        public bool PublishedToGovAndUpToDate { get; set; }
    }
}