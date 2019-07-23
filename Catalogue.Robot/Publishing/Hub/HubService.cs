using Catalogue.Data.Model;
using Catalogue.Robot.Publishing.Client;
using log4net;
using System;
using System.Net;

namespace Catalogue.Robot.Publishing.Hub
{
    public interface IHubService
    {
        void Publish(Record record);
        // void Delete(Record record); ?
        void Index(Record record);
    }

    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));

        private readonly Env env;
        private readonly IApiClient apiClient;
        private readonly IQueueClient queueClient;
        private readonly IHubMessageConverter hubMessageConverter;

        public HubService(Env env, IApiClient apiClient, IQueueClient queueClient)
        {
            this.env = env;
            this.apiClient = apiClient;
            this.queueClient = queueClient;
            this.hubMessageConverter = new HubMessageConverter(env, new FileHelper());
        }

        public void Publish(Record record)
        {
            var messageBody = hubMessageConverter.ConvertRecordToHubAsset(record);
            Logger.Debug($"Hub asset to send: {messageBody}");

            var response = apiClient.SendToHub(messageBody);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Error publishing the record to the ResourceHub: {response}");
            }

            Logger.Info($"Posted asset to Hub API endpoint {env.HUB_API_ENDPOINT}, response is {(int)response.StatusCode} {response.StatusCode}");
        }

        public void Index(Record record)
        {
            var message = hubMessageConverter.ConvertRecordToQueueMessage(record);
            //Logger.Debug($"Queue message to send: {message}");

            queueClient.Send(message);
        }
    }
}
