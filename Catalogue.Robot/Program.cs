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

            if (args.First() == "mark")
            {
                string keyword = args.Skip(1).First();
                MarkRecordsAsPublishableToDataGovUk(keyword);
            }

            if (args.First() == "publish")
            {
                Publish1000RecordsToDataGovUk();
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

                    record.Publication.DataGovUkPublisher = new DataGovUkPublisher();
                }

                db.SaveChanges();
                Console.WriteLine("Marked {0} records.", records.Count);                
            }
        }

        static void Publish1000RecordsToDataGovUk()
        {
            // load the config
            var configPath = Path.Combine(Environment.CurrentDirectory, "data-gov-uk-publisher-config.json");
            if (!File.Exists(configPath))
                throw new Exception("No data-gov-uk-publisher-config.json file in current directory.");
            string configJson = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<DataGovUkPublisherConfig>(configJson);

            using (var db = DocumentStore.OpenSession())
            {
                var recordIds = db.Query<Record>()
                    .Where(r => r.Publication.DataGovUkPublisher != null)
                    .Take(1000)
                    .ToList()
                    .Select(r => r.Id) // why can't ravendb manage to project the id?!
                    .ToList();

                foreach (var recordId in recordIds)
                {
                    Console.WriteLine("Publishing record {0}", recordId);
                    PublishRecordToDataGovUk(recordId, config);
                }
                Console.WriteLine("Published {0} records to data.gov.uk.", recordIds.Count);
            }
        }

        static void PublishRecordToDataGovUk(Guid recordId, DataGovUkPublisherConfig config)
        {
            using (var db = DocumentStore.OpenSession())
            {
                var record = db.Load<Record>(recordId);
                var publisher = record.Publication.DataGovUkPublisher;

                Console.WriteLine("Publishing '{0}' to '{1}'.", record.Gemini.Title, config.FtpRootUrl);

                // note the attempt
                if (publisher.Attempts == null)
                    publisher.Attempts = new List<PublicationAttempt>();
                var attempt = new PublicationAttempt { DateUtc = DateTime.UtcNow };
                publisher.Attempts.Add(attempt);
                db.SaveChanges();

                var c = new WebClient { Credentials = new NetworkCredential(config.FtpUsername, config.FtpPassword) };

                // upload the data file
                string dataFilename = WebUtility.UrlEncode(Path.GetFileName(record.Path));
                string dataFtpPath = String.Format("{0}/{1}/{2}", config.FtpRootUrl, record.Id, dataFilename);
                string dataHttpPath = String.Format("{0}/{1}/{2}", config.HttpRootUrl, record.Id, dataFilename);
                c.UploadFile(dataFtpPath, "STOR", record.Path);

                // update the resource locator in the metadata record
                record.Gemini.ResourceLocator = dataHttpPath;

                // upload the metadata document
                string metaFtpPath = String.Format("{0}/{1}", config.FtpRootUrl, record.Id + ".xml");
                string metaXmlDoc = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini).ToString();
                c.UploadString(metaFtpPath, "STOR", metaXmlDoc);

                // update the index.html
                string indexDocFtpPath = String.Format("{0}/index.html", config.FtpRootUrl);
                string indexDocHtml = c.DownloadString(indexDocFtpPath);
                var html = XDocument.Parse(indexDocHtml);
                var body = html.Element("body");
                body.Add(new XElement("a", new XAttribute("href", record.Id + ".xml")));
                c.UploadString(indexDocFtpPath, "STOR", html.ToString());

                // mark the attempt successful
                attempt.Successful = true;

                // commit the changes (to the resource locator and the attempt log)
                db.SaveChanges();
            }
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
