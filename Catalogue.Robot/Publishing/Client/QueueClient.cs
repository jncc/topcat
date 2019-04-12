using System;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.ExtendedClient;
using Amazon.SQS.Model;
using log4net;
using System.Threading.Tasks;

namespace Catalogue.Robot.Publishing.Client
{
    public interface IQueueClient
    {
        void Send(string message);
    }

    public class QueueClient : IQueueClient, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(QueueClient));

        private readonly Env env;
        private readonly AmazonS3Client s3;
        private readonly AmazonSQSClient sqs;
        private readonly AmazonSQSExtendedClient client;

        public QueueClient(Env env)
        {
            this.env = env;
            var credentials = new BasicAWSCredentials(env.HUB_QUEUE_AWS_ACCESSKEY, env.HUB_QUEUE_AWS_SECRETACCESSKEY);
            var region = RegionEndpoint.GetBySystemName(env.HUB_QUEUE_AWS_REGION);

            this.s3 = new AmazonS3Client(credentials, region);
            this.sqs = new AmazonSQSClient(credentials, region);
            this.client = new AmazonSQSExtendedClient(sqs, new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(s3, env.SQS_PAYLOAD_BUCKET));
        }

        public void Send(string message)
        {
            Logger.Info($"Attempting to send message to queue at endpoint {env.SQS_ENDPOINT}...");

            var result = SendMessage(client, env.SQS_ENDPOINT, message).GetAwaiter().GetResult();
            Logger.Info($"Successfully sent message to queue with ID {result.MessageId}");
        }

        public static async Task<SendMessageResponse> SendMessage(AmazonSQSExtendedClient client, string endpoint, string message)
        {
            return await client.SendMessageAsync(endpoint, message);
        }

        public void Dispose()
        {
            client.Dispose();
            sqs.Dispose();
            s3.Dispose();
        }
    }
}
