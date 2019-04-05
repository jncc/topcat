namespace Catalogue.Data.Model
{
    public class PublishingState
    {
        public bool AssessedAndUpToDate { get; set; }
        public bool SignedOffAndUpToDate { get; set; }
        public bool PublishedToHubAndUpToDate { get; set; }
        public bool PublishedToGovAndUpToDate { get; set; }
        public bool PublishedAndUpToDate { get; set; }
        public bool PreviouslyPublishedWithDoi { get; set; }
    }
}