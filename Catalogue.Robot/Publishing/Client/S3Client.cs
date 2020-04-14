using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Catalogue.Robot.Publishing.Client
{
    public interface IS3Client
    {
        PutObjectResponse SaveToS3(string recordId, string assetMessage);
    }

    public class S3Client : IS3Client
    {
        private readonly Env env;

        public S3Client(Env env)
        {
            this.env = env;
        }

        public PutObjectResponse SaveToS3(string recordId, string assetMessage)
        {
            using (var client = new AmazonS3Client(
                new BasicAWSCredentials(env.HUB_AWS_ACCESSKEY, env.HUB_AWS_SECRETACCESSKEY),
                RegionEndpoint.GetBySystemName(env.HUB_AWS_REGION)))
            {
                var request = new PutObjectRequest
                {
                    BucketName = env.HUB_LARGE_MESSAGE_BUCKET,
                    Key = recordId,
                    ContentBody = assetMessage
                };

                return client.PutObjectAsync(request).GetAwaiter().GetResult();
            }
        }
    }
}
