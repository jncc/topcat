using Catalogue.Data.Model;
using Catalogue.Robot.Publishing.Client;
using log4net;
using System;
using System.Net;

namespace Catalogue.Robot.Publishing.Hub
{
    public interface IHubService
    {
        void Save(Record record);
        // void Delete(Record record); ?
        void Index(Record record);
    }

    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));
        
        private readonly IApiClient apiClient;
        private readonly IQueueClient queueClient;
        private readonly HubMessageConverter hubMessageConverter;

        public HubService(Env env, IApiClient apiClient, IQueueClient queueClient)
        {
            this.apiClient = apiClient;
            this.queueClient = queueClient;
            this.hubMessageConverter = new HubMessageConverter(env, new FileHelper());
        }

        public void Save(Record record)
        {
            Logger.Info("Saving record as an asset to the Hub database");

            var messageBody = hubMessageConverter.ConvertRecordToHubAsset(record);
            //Logger.Debug($"Hub asset to send: {messageBody}");

            var response = apiClient.SendToHub(messageBody);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Error saving the record to the ResourceHub: {response}");
            }

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
