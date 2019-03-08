using dotenv.net;
using System;

namespace Catalogue.Robot.Publishing
{
    public class Env
    {
        public string HUB_ASSETS_BASE_URL { get; private set; }

        public string HUB_QUEUE_AWS_REGION { get; private set; }
        public string HUB_QUEUE_AWS_ACCESSKEY { get; private set; }
        public string HUB_QUEUE_AWS_SECRETACCESSKEY { get; private set; }
        public string SQS_ENDPOINT { get; private set; }
        public string SQS_PAYLOAD_BUCKET { get; private set; }

        public string HUB_API_AWS_REGION { get; private set; }
        public string HUB_API_AWS_ACCESSKEY { get; private set; }
        public string HUB_API_AWS_SECRETACCESSKEY { get; private set; }
        public string HUB_API_ENDPOINT { get; private set; }

        public Env()
        {
            DotEnv.Config();

            this.HUB_ASSETS_BASE_URL = GetVariable("HUB_ASSETS_BASE_URL");

            this.HUB_QUEUE_AWS_REGION = GetVariable("HUB_QUEUE_AWS_REGION");
            this.HUB_QUEUE_AWS_ACCESSKEY = GetVariable("HUB_QUEUE_AWS_ACCESSKEY");
            this.HUB_QUEUE_AWS_SECRETACCESSKEY = GetVariable("HUB_QUEUE_AWS_SECRETACCESSKEY");
            this.SQS_ENDPOINT = GetVariable("SQS_ENDPOINT");
            this.SQS_PAYLOAD_BUCKET = GetVariable("SQS_PAYLOAD_BUCKET");

            this.HUB_API_AWS_REGION = GetVariable("HUB_API_AWS_REGION");
            this.HUB_API_AWS_ACCESSKEY = GetVariable("HUB_API_AWS_ACCESSKEY");
            this.HUB_API_AWS_SECRETACCESSKEY = GetVariable("HUB_API_AWS_SECRETACCESSKEY");
            this.HUB_API_ENDPOINT = GetVariable("HUB_API_ENDPOINT");
        }

        string GetVariable(string name, bool required = true, string defaultValue = null)
        {
            string value = Environment.GetEnvironmentVariable(name) ?? defaultValue;

            if (required && value == null)
                throw new Exception($"Environment variable {name} is required but not set.");

            return value;
        }
    }
}
