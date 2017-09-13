using Catalogue.Data.Model;

namespace Catalogue.Robot.Publishing.OpenData
{
    interface IOpenDataXmlHelper
    {
        byte[] GetMetadataDocument(Record record, string resourceUrl);
        string UpdateWafIndexDocument(Record record, string indexDocHtml);
    }
}
