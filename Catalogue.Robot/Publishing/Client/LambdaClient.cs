using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;

namespace Catalogue.Robot.Publishing.Client
{
    public interface ILambdaClient
    {
        InvokeResponse SendToHub(string assetMessage);
    }

    public class LambdaClient : ILambdaClient
    {
        private readonly Env env;

        public LambdaClient(Env env)
        {
            this.env = env;
        }

        public InvokeResponse SendToHub(string assetMessage)
        {
            using (var client = new AmazonLambdaClient(
                new BasicAWSCredentials(env.HUB_AWS_ACCESSKEY, env.HUB_AWS_SECRETACCESSKEY),
                RegionEndpoint.GetBySystemName(env.HUB_AWS_REGION)))
            {
                var request = new InvokeRequest
                {
                    FunctionName = env.HUB_LAMBDA_FUNCTION,
                    Payload = assetMessage
                };

                return client.InvokeAsync(request).Result;
            }
        }
    }
}
