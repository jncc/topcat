using Catalogue.Data.Model;

namespace Catalogue.Robot.Publishing.Gov
{
    interface IXmlHelper
    {
        byte[] GetMetadataDocument(Record record);
        string UpdateWafIndexDocument(Record record, string indexDocHtml);
    }
}
