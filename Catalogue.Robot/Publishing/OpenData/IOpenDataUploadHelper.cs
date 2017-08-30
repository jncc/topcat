using System;
using Catalogue.Data.Model;

namespace Catalogue.Robot.Publishing.OpenData
{
    public interface IOpenDataUploadHelper
    {
        void UploadDataFile(Guid recordId, string filePath, bool metadataOnly);
        void UploadAlternativeResources(Record record, bool metadataOnly);
        void UploadMetadataDocument(Record record);
        void UploadWafIndexDocument(Record record);
        string GetHttpRootUrl();
    }
}
