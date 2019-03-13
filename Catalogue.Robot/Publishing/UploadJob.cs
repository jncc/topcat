using System;
using System.IO;
using Catalogue.Data.Query;
using Catalogue.Data.Write;
using Catalogue.Robot.Publishing.Data;
using Catalogue.Robot.Publishing.Gov;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Utilities.Logging;
using Catalogue.Utilities.Text;
using log4net;
using Newtonsoft.Json;
using Quartz;
using Raven.Client.Documents;

namespace Catalogue.Robot.Publishing
{
    public class UploadJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UploadJob));

        readonly IDocumentStore store;

        public UploadJob(IDocumentStore store)
        {
            this.store = store;
        }

        public void Execute(IJobExecutionContext context)
        {
            Logger.Info("Executing job");
            RunUpload();
            Logger.Info("Finished all jobs");
        }

        public void RunUpload(bool metadataOnly = false)
        {
            using (var db = store.OpenSession())
            {
                var config = GetConfigFile();
                var publishingService = new RecordPublishingService(db, new RecordValidator());
                var uploadService = publishingService.Upload();
                var dataUploader = new DataUploader(config);
                var metadataUploader = new MetadataUploader(config);
                var recordRedactor = new RecordRedactor(new VocabQueryer(db));
                var hubService = new HubService(recordRedactor);

                var robotUploader = new RobotPublisher(db, recordRedactor, uploadService, dataUploader, metadataUploader, hubService);

                var records = robotUploader.GetRecordsPendingUpload();
                Logger.Info("Number of records to upload: " + records.Count);

                robotUploader.PublishRecords(records, metadataOnly);
            }
        }

        private UploaderConfig GetConfigFile()
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, "data-gov-uk-publisher-config.json");
            if (!File.Exists(configPath))
            {
                var e = new Exception("No data-gov-uk-publisher-config.json file in current directory.");
                e.LogAndThrow(Logger);
            }
            string configJson = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<UploaderConfig>(configJson);
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
