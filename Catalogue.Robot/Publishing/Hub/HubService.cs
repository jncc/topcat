using Catalogue.Data.Model;
using log4net;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));

        private readonly QueueClient queueClient;
        private readonly MessageHelper messageHelper;

        public HubService()
        {
            queueClient = new QueueClient();
            messageHelper = new MessageHelper();
        }

        public void Upsert(Record record)
        {
            // attempt saving to datahub db
            Logger.Info("Hub upsert operation not yet implemented");
        }

        public void Index(Record record)
        {
            // attempt indexing in elasticsearch
            Logger.Info("Attempting to add the record to the queue for search indexing");
            
            var message = messageHelper.ConvertRecordToQueueMessage(record);

            Logger.Debug($"Message to send: {message}");

            queueClient.Send(message);

            Logger.Info("Message successfully added to queue");
        }

        
    }
}
