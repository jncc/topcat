using Catalogue.Data.Model;
using log4net;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));

        private readonly HubApiHelper apiHelper;
        private readonly QueueClient queueClient;
        private readonly MessageHelper messageHelper;

        public HubService()
        {
            apiHelper = new HubApiHelper();
            queueClient = new QueueClient();
            messageHelper = new MessageHelper();
        }

        public void Upsert(Record record)
        {
            // attempt saving to datahub db
            Logger.Info("Saving record as an asset to the Hub database");
            apiHelper.Save(record);
        }

        public void Index(Record record)
        {
            // attempt indexing in elasticsearch
            Logger.Info("Attempting to add the record to the queue for search indexing");
            
            var message = messageHelper.ConvertRecordToQueueMessage(record);

            Logger.Debug($"Message to send: {message}");

            //queueClient.Send(message);

            Logger.Info("Message successfully added to queue");
        }
    }
}
