using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.ExtendedClient;
using Catalogue.Data.Model;
using dotenv.net;
using log4net;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));
        

        public void Upsert(Record record)
        {
            // attempt saving to datahub db
            Logger.Info("Hub upsert operation not yet implemented");
        }

        public void Index(Record record)
        {
            // attempt indexing in elasticsearch
            Logger.Info("Attempting to add the record to the ElasticSearch queue for indexing");

            //indexService.Upsert(record);

            var messageHelper = new MessageHelper();
            var message = messageHelper.ConvertRecordToQueueMessage(record);
            Logger.Info($"Message to put on queue: {message}");

            Logger.Info("Another message here");
            DotEnv.Config();
            
            var credentials = new BasicAWSCredentials(Environment.GetEnvironmentVariable("AWS_ACCESSKEY"), Environment.GetEnvironmentVariable("AWS_SECRETACCESSKEY"));
            var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));
            var s3 = new AmazonS3Client(credentials, region);
            var sqs = new AmazonSQSClient(credentials, region);
            var client = new AmazonSQSExtendedClient(sqs,
                new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(s3, Environment.GetEnvironmentVariable("SQS_PAYLOAD_BUCKET")));

            var endpoint = Environment.GetEnvironmentVariable("SQS_ENDPOINT");
            Logger.Info($"Attempting to send message to queue at endpoint {endpoint}...");

            SendMessage(client, endpoint, message).GetAwaiter().GetResult();

            Logger.Info("Message successfully added to queue");
        }

        public static async Task SendMessage(AmazonSQSExtendedClient client, string endpoint, string message)
        {
            var response = await client.SendMessageAsync(endpoint, message);

            Logger.Info($"Sent message to queue! ID is {response.MessageId}");
        }
    }
}
