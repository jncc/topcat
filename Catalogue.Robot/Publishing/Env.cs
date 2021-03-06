﻿using dotenv.net;
using System;

namespace Catalogue.Robot.Publishing
{
    public class Env
    {
        // data file host
        public string HTTP_ROOT_URL { get; private set; }
        public long MAX_FILE_SIZE_IN_BYTES { get; private set; }

        public string FTP_HOST { get; private set; }
        public string FTP_USERNAME { get; private set; }
        public string FTP_PASSWORD { get; private set; }
        public string FTP_DATA_FOLDER { get; private set; }
        public string FTP_WAF_FOLDER { get; private set; }
        public string FTP_ROLLBACK_FOLDER { get; private set; }

        // resource hub
        public string HUB_AWS_REGION { get; private set; }
        public string HUB_AWS_ACCESSKEY { get; private set; }
        public string HUB_AWS_SECRETACCESSKEY { get; private set; }
        public string HUB_ASSETS_BASE_URL { get; private set; }
        public string HUB_BASE_URL { get; private set; }
        public string HUB_DYNAMO_TABLE { get; private set; }
        public string HUB_LAMBDA_FUNCTION { get; private set; }
        public string HUB_LARGE_MESSAGE_BUCKET { get; private set; }

        public string SQS_ENDPOINT { get; private set; }
        public string SQS_PAYLOAD_BUCKET { get; private set; }
        public string ES_INDEX { get; private set; }
        public string ES_SITE { get; private set; }

        // emailing
        public bool SMTP_NOTIFICATIONS_ON { get; private set; }
        public string SMTP_HOST { get; private set; }
        public string SMTP_FROM { get; private set; }
        public string SMTP_TO { get; private set; }

        public Env(string filePath = ".env")
        {
            DotEnv.Config(filePath: filePath);

            this.HTTP_ROOT_URL = GetVariable("HTTP_ROOT_URL");
            this.MAX_FILE_SIZE_IN_BYTES = long.Parse(GetVariable("MAX_FILE_SIZE_IN_BYTES"));

            this.FTP_HOST = GetVariable("FTP_HOST");
            this.FTP_USERNAME = GetVariable("FTP_USERNAME");
            this.FTP_PASSWORD = GetVariable("FTP_PASSWORD");
            this.FTP_DATA_FOLDER = GetVariable("FTP_DATA_FOLDER");
            this.FTP_WAF_FOLDER = GetVariable("FTP_WAF_FOLDER");
            this.FTP_ROLLBACK_FOLDER = GetVariable("FTP_ROLLBACK_FOLDER");

            this.HUB_AWS_REGION = GetVariable("HUB_AWS_REGION");
            this.HUB_AWS_ACCESSKEY = GetVariable("HUB_AWS_ACCESSKEY");
            this.HUB_AWS_SECRETACCESSKEY = GetVariable("HUB_AWS_SECRETACCESSKEY");
            this.HUB_ASSETS_BASE_URL = GetVariable("HUB_ASSETS_BASE_URL");
            this.HUB_BASE_URL = GetVariable("HUB_BASE_URL");
            this.HUB_DYNAMO_TABLE = GetVariable("HUB_DYNAMO_TABLE");
            this.HUB_LAMBDA_FUNCTION = GetVariable("HUB_LAMBDA_FUNCTION");
            this.HUB_LARGE_MESSAGE_BUCKET = GetVariable("HUB_LARGE_MESSAGE_BUCKET");

            this.SQS_ENDPOINT = GetVariable("SQS_ENDPOINT");
            this.SQS_PAYLOAD_BUCKET = GetVariable("SQS_PAYLOAD_BUCKET");
            this.ES_INDEX = GetVariable("ES_INDEX");
            this.ES_SITE = GetVariable("ES_SITE");

            this.SMTP_NOTIFICATIONS_ON = bool.Parse(GetVariable("SMTP_NOTIFICATIONS_ON"));
            this.SMTP_HOST = GetVariable("SMTP_HOST");
            this.SMTP_FROM = GetVariable("SMTP_FROM");
            this.SMTP_TO = GetVariable("SMTP_TO");
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
