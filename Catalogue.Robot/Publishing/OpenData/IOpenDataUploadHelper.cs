using System;
using Catalogue.Data.Model;

namespace Catalogue.Robot.Publishing.OpenData
{
    public interface IOpenDataUploadHelper
    {
        void UploadDataFile(Guid recordId, string filePath);
        void UploadAlternativeResources(Record record);
        void UploadMetadataDocument(Record record);
        void UploadWafIndexDocument(Record record);
        string GetHttpRootUrl();
    }
}
