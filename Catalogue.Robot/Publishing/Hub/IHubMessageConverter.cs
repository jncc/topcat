using Catalogue.Data.Model;

namespace Catalogue.Robot.Publishing.Hub
{
    public interface IHubMessageConverter
    {
        string ConvertRecordToHubAsset(Record record);
        string ConvertRecordToQueueMessage(Record record);
    }
}
