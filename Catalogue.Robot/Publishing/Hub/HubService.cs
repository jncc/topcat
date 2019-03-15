using Catalogue.Data.Model;
using log4net;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));
        
        private readonly HubApiHelper apiHelper;
        private readonly QueueClient queueClient;
        private readonly HubMessageConverter hubMessageConverter;

        public HubService(Env env)
        {
            this.apiHelper = new HubApiHelper(env);
            this.queueClient = new QueueClient(env);
            this.hubMessageConverter = new HubMessageConverter(env, new FileHelper());
        }

        public void Save(Record record)
        {
            Logger.Info("Saving record as an asset to the Hub database");

            var messageBody = hubMessageConverter.ConvertRecordToHubAsset(record);
            //Logger.Debug($"Hub asset to send: {messageBody}");

            apiHelper.SendMessage(messageBody);

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
