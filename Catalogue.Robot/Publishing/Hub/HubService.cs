using Catalogue.Data.Model;
using Catalogue.Robot.Publishing.Client;
using log4net;
using System;
using System.Net;
using Catalogue.Data;

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
        private readonly IS3Client s3Client;
        private readonly IFileHelper fileHelper;
        private readonly IHubMessageConverter hubMessageConverter;

        public HubService(Env env, ILambdaClient lambdaClient, IS3Client s3Client, IFileHelper fileHelper)
        {
            this.env = env;
            this.lambdaClient = lambdaClient;
            this.s3Client = s3Client;
            this.fileHelper = fileHelper;
            this.hubMessageConverter = new HubMessageConverter(env, fileHelper);
        }

        public void Publish(Record record)
        {
            var recordHasAPdfResource = false;
            foreach (var resource in record.Resources)
            {
                if (fileHelper.IsPdfFile(resource.Path))
                {
                    recordHasAPdfResource = true;
                    break;
                }
            }

            if (recordHasAPdfResource)
            {
                var recordId = Helpers.RemoveCollection(record.Id);
                var largeMessage = hubMessageConverter.ConvertRecordToHubPublishMessage(record);
                SaveLargeMessage(recordId, largeMessage);

                var message = hubMessageConverter.GetS3PublishMessage(recordId);
                SendToHubIngester(message);
            }
            else
            {
                var message = hubMessageConverter.ConvertRecordToHubPublishMessage(record);
                SendToHubIngester(message);
            }
        }

        private void SendToHubIngester(string message)
        {
            Logger.Info($"Sending hub message to {env.HUB_LAMBDA_FUNCTION} lambda");
            var response = lambdaClient.SendToHub(message);

            if (response.StatusCode != 200 || response.FunctionError != null)
            {
                throw new InvalidOperationException($"Error sending message to the Resource hub: {response.StatusCode} {response.FunctionError}");
            }
        }

        private void SaveLargeMessage(string recordId, string message)
        {
            Logger.Info($"Saving large message to {env.HUB_LARGE_MESSAGE_BUCKET} bucket");
            var response = s3Client.SaveToS3(recordId, message);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Error saving large message to the Resource hub S3 bucket: {response.HttpStatusCode}");
            }
        }
    }
}
