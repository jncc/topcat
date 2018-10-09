using System;
using Catalogue.Data.Model;

namespace Catalogue.Robot.Publishing.OpenData
{
    public interface IOpenDataUploadHelper
    {
        void UploadDataFile(string recordId, string filePath);
        void UploadMetadataDocument(Record record);
        void UploadWafIndexDocument(Record record);
        string GetHttpRootUrl();
    }
}
