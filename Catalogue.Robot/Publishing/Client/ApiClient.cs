using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Aws4RequestSigner;

namespace Catalogue.Robot.Publishing.Client
{
    public class ApiClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ApiClient));

        private readonly Env env;

        public ApiClient(Env env)
        {
            this.env = env;
        }

        public void SendToHub(string assetMessage)
        {
            var response = Post(assetMessage).GetAwaiter().GetResult();
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
