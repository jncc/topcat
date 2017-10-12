using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IOpenDataPublishingUploadRecordService
    {
        void UpdateLastAttempt(Record record, PublicationAttempt attempt);
        void UpdateLastSuccess(Record record, PublicationAttempt attempt);
        void UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(Record record);
        void UpdateResourceLocatorToMatchMainDataFile(Record record, string dataHttpPath);
    }
}
