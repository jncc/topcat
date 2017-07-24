using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IOpenDataPublishingService
    {
        // TODO
        // Assess
        void SignOff(Record record, OpenDataSignOffInfo signOffInfo);
    }
}