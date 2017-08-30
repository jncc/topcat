using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Text;
using Newtonsoft.Json;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.IO;

namespace Catalogue.Robot
{
    public class Robot
    {
        // todo: split "instance" stuff out into a separate class

        readonly IDocumentStore store;

        public Robot(IDocumentStore store)
        {
            this.store = store;
        }

        public void Start()
        {
            Console.WriteLine("Running Open Data Uploader");
            using (var db = store.OpenSession())
            {
                var records = GetRecordsPendingUpload(db);
                var config = GetConfigFile();
                var uploadService = new OpenDataPublishingUploadService(db, new RecordService(db, new RecordValidator()));
                var uploadHelper = new OpenDataUploadHelper(config);

                var uploader = new RobotUploader(db, uploadService, uploadHelper);

                uploader.Upload(records);
            }

            Console.WriteLine("Finished all jobs");
        }

        public void Stop()
        {
            Console.WriteLine("I'm stopping");
        }

        private OpenDataPublisherConfig GetConfigFile()
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, "data-gov-uk-publisher-config.json");
            if (!File.Exists(configPath))
                throw new Exception("No data-gov-uk-publisher-config.json file in current directory.");
            string configJson = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<OpenDataPublisherConfig>(configJson);
            if (config.FtpRootUrl.IsBlank())
                throw new Exception("No FtpRootUrl specified in data-gov-uk-publisher-config.json file.");
            if (config.HttpRootUrl.IsBlank())
                throw new Exception("No HttpRootUrl specified in data-gov-uk-publisher-config.json file.");
            if (config.FtpUsername.IsBlank())
                throw new Exception("No FtpUsername specified in data-gov-uk-publisher-config.json file.");
            if (config.FtpPassword.IsBlank())
                throw new Exception("No FtpPassword specified in data-gov-uk-publisher-config.json file.");

            return config;
        }

        private List<Record> GetRecordsPendingUpload(IDocumentSession db)
        {
            var records = new List<Record>(new [] {db.Load<Record>(new Guid("b2691fed-e421-4e48-9da9-99bd77e0b8ba")) });

            return records;
        }
    }
}
