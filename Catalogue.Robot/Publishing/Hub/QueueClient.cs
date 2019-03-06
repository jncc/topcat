using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.ExtendedClient;
using dotenv.net;
using log4net;
using System;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace Catalogue.Robot.Publishing.Hub
{
    public class QueueClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(QueueClient));

        private readonly AmazonSQSExtendedClient client;
        private readonly string endpoint;

        public QueueClient()
        {
            DotEnv.Config();

            var credentials = new BasicAWSCredentials(Environment.GetEnvironmentVariable("AWS_ACCESSKEY"), Environment.GetEnvironmentVariable("AWS_SECRETACCESSKEY"));
            var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));
            var s3 = new AmazonS3Client(credentials, region);
            var sqs = new AmazonSQSClient(credentials, region);
            client = new AmazonSQSExtendedClient(sqs,
                new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(s3, Environment.GetEnvironmentVariable("SQS_PAYLOAD_BUCKET")));

            endpoint = Environment.GetEnvironmentVariable("SQS_ENDPOINT");
        }

        public void Send(string message)
        {
            Logger.Info($"Attempting to send message to queue at endpoint {endpoint}...");
            var result = SendMessage(client, endpoint, message).GetAwaiter().GetResult();

            Logger.Info($"Successfully sent message to queue with ID {result.MessageId}");
        }

        public static async Task<SendMessageResponse> SendMessage(AmazonSQSExtendedClient client, string endpoint, string message)
        {
            return await client.SendMessageAsync(endpoint, message);
        }
    }
}
