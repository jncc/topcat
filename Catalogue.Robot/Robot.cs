using Catalogue.Data.Write;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Text;
using Newtonsoft.Json;
using Raven.Client;
using System;
using System.IO;
using Catalogue.Utilities.Logging;
using log4net;
using log4net.Config;

namespace Catalogue.Robot
{
    public class Robot
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Robot));

        readonly IDocumentStore store;

        public Robot(IDocumentStore store)
        {
            XmlConfigurator.Configure();
            this.store = store;
        }

        public void Start()
        {
            logger.Info("Starting Robot");
            using (var db = store.OpenSession())
            {
                var config = GetConfigFile();
                var uploadService = new OpenDataPublishingUploadService(new RecordService(db, new RecordValidator()));
                var uploadHelper = new OpenDataUploadHelper(config);

                var robotUploader = new RobotUploader(db, uploadService, uploadHelper);

                var records = robotUploader.GetRecordsPendingUpload();
                logger.Info("Number of records to upload: " + records.Count);

                robotUploader.Upload(records);
            }

            logger.Info("Finished all jobs");
        }

        public void Stop()
        {
            logger.Info("Stopping Robot");
        }

        private OpenDataPublisherConfig GetConfigFile()
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, "data-gov-uk-publisher-config.json");
            if (!File.Exists(configPath))
            {
                var e = new Exception("No data-gov-uk-publisher-config.json file in current directory.");
                e.LogAndThrow(logger);
            }
            string configJson = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<OpenDataPublisherConfig>(configJson);
            if (config.FtpRootUrl.IsBlank())
            {
                var e = new Exception("No FtpRootUrl specified in data-gov-uk-publisher-config.json file.");
                e.LogAndThrow(logger);
            }
            if (config.HttpRootUrl.IsBlank())
            {
                var e = new Exception("No HttpRootUrl specified in data-gov-uk-publisher-config.json file.");
                e.LogAndThrow(logger);
            }
            if (config.FtpUsername.IsBlank())
            {
                var e = new Exception("No FtpUsername specified in data-gov-uk-publisher-config.json file.");
                e.LogAndThrow(logger);
            }
            if (config.FtpPassword.IsBlank())
            {
                var e = new Exception("No FtpPassword specified in data-gov-uk-publisher-config.json file.");
                e.LogAndThrow(logger);
            }

            return config;
        }
    }
}
