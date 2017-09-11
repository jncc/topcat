using System;
using System.IO;
using Catalogue.Data.Write;
using Catalogue.Utilities.Logging;
using Catalogue.Utilities.Text;
using log4net;
using Newtonsoft.Json;
using Quartz;
using Raven.Client;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class OpenDataUploadJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Robot));

        readonly IDocumentStore store;

        public OpenDataUploadJob(IDocumentStore store)
        {
            this.store = store;
        }

        public void Execute(IJobExecutionContext context)
        {
            Logger.Info("Executing job");
            Logger.Info(store == null ? "Doc store is null" : "Store not null");
            try
            {
                store.Initialize();
                using (var db = store.OpenSession())
                {
                    Logger.Info("Using db");
                    var config = GetConfigFile();
                    var publishingService = new OpenDataPublishingService(new RecordService(db, new RecordValidator()));
                    var uploadService = publishingService.Upload();
                    var uploadHelper = new OpenDataUploadHelper(config);

                    var robotUploader = new RobotUploader(db, uploadService, uploadHelper);

                    var records = robotUploader.GetRecordsPendingUpload();
                    Logger.Info("Number of records to upload: " + records.Count);

                    robotUploader.Upload(records);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            Logger.Info("Finished all jobs");
        }

        private OpenDataUploadConfig GetConfigFile()
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, "data-gov-uk-publisher-config.json");
            if (!File.Exists(configPath))
            {
                var e = new Exception("No data-gov-uk-publisher-config.json file in current directory.");
                e.LogAndThrow(Logger);
            }
            string configJson = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<OpenDataUploadConfig>(configJson);
            if (config.FtpRootUrl.IsBlank())
            {
                var e = new Exception("No FtpRootUrl specified in data-gov-uk-publisher-config.json file.");
                e.LogAndThrow(Logger);
            }
            if (config.HttpRootUrl.IsBlank())
            {
                var e = new Exception("No HttpRootUrl specified in data-gov-uk-publisher-config.json file.");
                e.LogAndThrow(Logger);
            }
            if (config.FtpUsername.IsBlank())
            {
                var e = new Exception("No FtpUsername specified in data-gov-uk-publisher-config.json file.");
                e.LogAndThrow(Logger);
            }
            if (config.FtpPassword.IsBlank())
            {
                var e = new Exception("No FtpPassword specified in data-gov-uk-publisher-config.json file.");
                e.LogAndThrow(Logger);
            }

            return config;
        }
    }
}
