using Catalogue.Data.Model;
using Raven.Client;
using System;
using System.IO;
using System.Linq;
using Catalogue.Data.Write;
using Catalogue.Utilities.Text;
using Newtonsoft.Json;

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
            Console.WriteLine("I'm a robot");

            using (var db = store.OpenSession())
            {
                var record = db.Load<Record>(new Guid("b2691fed-e421-4e48-9da9-99bd77e0b8ba"));
                Console.WriteLine(record.Gemini.Title);

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

                var recordService = new RecordService(db, new RecordValidator());
                var uploadService = new OpenDataUploadService(config);
                var publishingService = new OpenDataPublishingService(db, recordService, uploadService);
                var userInfo = new UserInfo
                {
                    DisplayName = "Robot Test Uploader",
                    Email = "testemail@company.com"
                };
                
                publishingService.Upload(record, userInfo, false);
            }
        }

        public void Stop()
        {
            Console.WriteLine("I'm stopping");
        }


    }
}
