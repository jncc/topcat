using System;
using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IOpenDataUploadService
    {
        void UploadDataFile(Guid recordId, string path, bool metadataOnly);
        void UploadAlternativeResources(Record record, bool metadataOnly);
        void UploadMetadataDocument(Record record);
        void UploadWafIndexDocument(Record record);
        string GetHttpRootUrl();
    }
}
