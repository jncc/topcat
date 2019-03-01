using System;
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
    public class SearchIndexService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SearchIndexService));

        private readonly MessageHelper messageHelper;
        private readonly AmazonSQSExtendedClient client;

        public SearchIndexService()
        {
            Logger.Info("Initialising search index service");
            //DotEnv.Config();

            messageHelper = new MessageHelper();

            //var credentials = new BasicAWSCredentials(Environment.GetEnvironmentVariable("AWS_ACCESSKEY"), Environment.GetEnvironmentVariable("AWS_SECRETACCESSKEY"));
            //var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));
            //var s3 = new AmazonS3Client(credentials, region);
            //var sqs = new AmazonSQSClient(credentials, region);
            //client = new AmazonSQSExtendedClient(sqs,
            //    new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(s3, Environment.GetEnvironmentVariable("SQS_PAYLOAD_BUCKET")));
            Logger.Info("Finished initialising");
        }

        public async void Upsert(Record record)
        {
            var message = messageHelper.ConvertRecordToQueueMessage(record);

            //var response = await client.SendMessageAsync(Environment.GetEnvironmentVariable("SQS_ENDPOINT"), message);

            //Logger.Info($"Sent message to queue, ID is {response.MessageId}");
        }
    }
}
