using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aws4RequestSigner;
using Catalogue.Data.Model;
using log4net;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubApiHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubApiHelper));
        private static Env env = new Env();

        private readonly MessageHelper messageHelper;

        public HubApiHelper()
        {
            this.messageHelper = new MessageHelper();
        }

        public void Save(Record record)
        {
            var messageBody = messageHelper.ConvertRecordToHubAsset(record);
            Logger.Debug($"Converted record to hub asset: {messageBody}");

            var response = Post(messageBody).GetAwaiter().GetResult();

            Logger.Info($"Posted asset to Hub API endpoint {env.HUB_API_ENDPOINT}, response is {response}");
        }

        private async Task<string> Post(string body)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(env.HUB_API_ENDPOINT),
                Content = new StringContent(
                    body,
                    Encoding.UTF8,
                    "application/json"
                )
            };

            var signedRequest = await GetSignedRequest(request);
            var response = await new HttpClient().SendAsync(signedRequest);
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        private async Task<HttpRequestMessage> GetSignedRequest(HttpRequestMessage request)
        {
            var signer = new AWS4RequestSigner(env.HUB_API_AWS_ACCESSKEY, env.HUB_API_AWS_SECRETACCESSKEY);
            return await signer.Sign(request, "execute-api", env.HUB_API_AWS_REGION);
        }
    }
}
