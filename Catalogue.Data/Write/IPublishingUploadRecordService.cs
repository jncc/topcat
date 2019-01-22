using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IPublishingUploadRecordService
    {
        void UpdateLastAttempt(Record record, PublicationAttempt attempt);
        void UpdateLastSuccess(Record record, PublicationAttempt attempt);
        void UpdatePublishedUrlForResource(Resource resource, string dataHttpPath);
    }
}
