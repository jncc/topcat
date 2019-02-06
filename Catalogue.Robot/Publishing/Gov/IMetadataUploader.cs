using Catalogue.Data.Model;

namespace Catalogue.Robot.Publishing.Gov
{
    public interface IMetadataUploader
    {
        void UploadMetadataDocument(Record record);
        void UploadWafIndexDocument(Record record);
    }
}
