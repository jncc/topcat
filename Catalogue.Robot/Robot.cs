using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Text;
using Newtonsoft.Json;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catalogue.Data.Indexes;

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
                var config = GetConfigFile();
                var uploadService = new OpenDataPublishingUploadService(new RecordService(db, new RecordValidator()));
                var uploadHelper = new OpenDataUploadHelper(config);

                var robotUploader = new RobotUploader(db, uploadService, uploadHelper);

                var records = robotUploader.GetRecordsPendingUpload();
                Console.WriteLine("Number of records to upload: " + records.Count);

                robotUploader.Upload(records);
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
    }
}
