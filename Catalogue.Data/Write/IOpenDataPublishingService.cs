using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IOpenDataPublishingService
    {
        void SignOff(Record record, OpenDataSignOffInfo signOffInfo);
        // TODO
        // Assess
        // Publish
    }
}