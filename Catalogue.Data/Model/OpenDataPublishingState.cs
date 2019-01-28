namespace Catalogue.Web.Controllers.Records
{
    public class OpenDataPublishingState
    {
        public bool AssessedAndUpToDate { get; set; }
        public bool SignedOffAndUpToDate { get; set; }
        public bool PublishedToHubAndUpToDate { get; set; }
        public bool PublishedToGovAndUpToDate { get; set; }
    }
}