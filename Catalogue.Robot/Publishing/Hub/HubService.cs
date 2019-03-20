using Catalogue.Data.Model;
using Catalogue.Robot.Publishing.Client;
using log4net;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));
        
        private readonly ApiClient apiClient;
        private readonly QueueClient queueClient;
        private readonly HubMessageConverter hubMessageConverter;

        public HubService(Env env)
        {
            this.apiClient = new ApiClient(env);
            this.queueClient = new QueueClient(env);
            this.hubMessageConverter = new HubMessageConverter(env, new FileHelper());
        }

        public void Save(Record record)
        {
            Logger.Info("Saving record as an asset to the Hub database");

            var messageBody = hubMessageConverter.ConvertRecordToHubAsset(record);
            //Logger.Debug($"Hub asset to send: {messageBody}");

            apiClient.SendToHub(messageBody);

            Logger.Info("Message posted to hub API endpoint");
        }

        public void Index(Record record)
        {
            Logger.Info("Attempting to add the record to the queue for search indexing");
            
            var message = hubMessageConverter.ConvertRecordToQueueMessage(record);
            //Logger.Debug($"Queue message to send: {message}");

            queueClient.Send(message);

            Logger.Info("Message added to queue");
        }
    }
}
