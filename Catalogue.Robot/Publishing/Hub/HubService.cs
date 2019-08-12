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
    }

    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));

        private readonly Env env;
        private readonly ILambdaClient lambdaClient;
        private readonly IHubMessageConverter hubMessageConverter;

        public HubService(Env env, ILambdaClient lambdaClient)
        {
            this.env = env;
            this.lambdaClient = lambdaClient;
            this.hubMessageConverter = new HubMessageConverter(env, new FileHelper());
        }

        public void Publish(Record record)
        {
            var messageBody = hubMessageConverter.ConvertRecordToHubMessage(record);

            Logger.Info($"Sending hub message to {env.HUB_LAMBDA_FUNCTION} lambda");
            var response = lambdaClient.SendToHub(messageBody);

            if (response.StatusCode != 200)
            {
                throw new InvalidOperationException($"Error publishing the record to the ResourceHub: {response}");
            }
            
        }
    }
}
