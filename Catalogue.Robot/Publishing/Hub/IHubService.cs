using Catalogue.Data.Model;

namespace Catalogue.Robot.Publishing.Hub
{
    public interface IHubService
    {
        void Upsert(Record record);
        // void Delete(Record record); ?
        void Index(Record record);
    }
}