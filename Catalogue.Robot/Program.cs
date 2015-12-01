using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Robot.Importing;
using Catalogue.Robot.Publishing.DataGovUk;
using Newtonsoft.Json;
using Raven.Client;
using Raven.Client.Document;

namespace Catalogue.Robot
{
    class Program
    {
        public static IDocumentStore DocumentStore { get; private set; }

        static void Main(string[] args)
        {
            Init();

            if (args.First() == "import")
            {
                using (var db = DocumentStore.OpenSession())
                {
                    new ImportHandler(db).Import(args);
                }
            }
            else if (args.First() == "mark")
            {
                string keyword = args.Skip(1).First();
                MarkRecordsAsPublishableToDataGovUk(keyword);
            }
            else if (args.First() == "publish")
            {
                Publish1RecordsToDataGovUk();
            }
        }

        static void MarkRecordsAsPublishableToDataGovUk(string keyword)
        {
            using (var db = DocumentStore.OpenSession())
            {
                var records = GetRecords(db, keyword);

                // in reality need to go through RecordService and ensure appropriate validation etc.
                foreach (var record in records)
                {
                    if (record.Publication == null)
                        record.Publication = new PublicationInfo();

                    if (record.Publication.DataGovUk == null)
                        record.Publication.DataGovUk = new DataGovUkPublicationInfo();
                }

                db.SaveChanges();
                Console.WriteLine("Marked {0} records.", records.Count);                
            }
        }

        static void Publish1RecordsToDataGovUk()
        {
            // load the config
            var configPath = Path.Combine(Environment.CurrentDirectory, "data-gov-uk-publisher-config.json");
            if (!File.Exists(configPath))
                throw new Exception("No data-gov-uk-publisher-config.json file in current directory.");
            string configJson = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<DataGovUkPublisherConfig>(configJson);

            var ids = new List<Guid>();
            using (var db = DocumentStore.OpenSession())
            {
                ids = db.Query<Record>()
                    .Where(r => r.Publication.DataGovUk != null)
                    .Take(1)
                    .ToList()
                    .Select(r => r.Id) // why can't ravendb manage to project the id?!
                    .ToList();
            }

                foreach (var id in ids)
                {
                    var ftpClient = new FtpClient(config.FtpUsername, config.FtpPassword);
                    using (var db = DocumentStore.OpenSession())
                    {
                        new RecordPublisher(db, config, ftpClient).PublishRecord(id);
                    }
                }

                Console.WriteLine("Published {0} records to data.gov.uk.", ids.Count);
        }


        static List<Record> GetRecords(IDocumentSession db, string keyword)
        {
            var recordQueryer = new RecordQueryer(db);
            var query = new RecordQueryInputModel
            {
                K = new[] { keyword },
                N = 1024,
            };

            int count = recordQueryer.Query(query).Count();

            var records = recordQueryer.Query(query).ToList();

            if (records.Count != count)
                throw new Exception("Too many records.");

            return records;
        }

        static void Init()
        {
            try
            {
                // for local dev, we expect to use the db running in the local dev web app,
                // so the DatabaseFactory just uses the dev connection string in the dev app.config file
                DocumentStore = DatabaseFactory.Production();
            }
            catch (HttpRequestException ex)
            {                
                throw new Exception("Unable to connect to the Topcat database.", ex);
            }
        }
    }
}
