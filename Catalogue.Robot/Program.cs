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
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Robot.Importing;
using Catalogue.Robot.Publishing.OpenData;
using CommandLine;
using Newtonsoft.Json;
using Raven.Client;

namespace Catalogue.Robot
{
    [Verb("import", HelpText = "Import records from a CSV file.")]
    public class ImportOptions
    {
        [Option(Required = true, HelpText = "Import mapping, e.g. 'StandardTopcatMapping'.")]
        public string Mapping { get; set; }

        [Option(Required = true, HelpText = "Path to the CSV file to import.")]
        public string File { get; set; }

        [Option("skip-bad-records", Default = false, HelpText = "Skip bad records?")]
        public bool SkipBadRecords { get; set; }
    }

    [Verb("publish", HelpText = "Mark records tagged with the specified keyword for publishing.")]
    public class PublishOptions
    {
        [Option(Required = true, HelpText = "Keyword, e.g. 'vocab.jncc.gov.uk/jncc-category/Some Category'.")]
        public string Keyword { get; set; }

        [Option("now", Default = false, HelpText = "Publish immediately")]
        public bool Now { get; set; }

    }


    class Program
    {
        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<ImportOptions, PublishOptions>(args).MapResult(
                (ImportOptions options) => RunImportAndReturnExitCode(options),
                (PublishOptions options) => RunPublishAndReturnExitCode(options),
                errs => 1);
        }

        public static IDocumentStore DocumentStore { get; private set; }

        static int RunImportAndReturnExitCode(ImportOptions options)
        {
            InitDatabase();

            using (var db = DocumentStore.OpenSession())
            {
                new ImportHandler(db).Import(options);
            }

            return 0;
        }

        static int RunPublishAndReturnExitCode(PublishOptions options)
        {
            InitDatabase();

            using (var db = DocumentStore.OpenSession())
            {
                var records = GetRecords(db, options.Keyword);

                Console.WriteLine("Found {0} records tagged '{1}'.", records.Count, options.Keyword);

                foreach (var record in records)
                {
                    if (record.Publication == null)
                        record.Publication = new PublicationInfo();

                    if (record.Publication.OpenData == null)
                        record.Publication.OpenData = new OpenDataPublicationInfo();
                }

                db.SaveChanges();
                Console.WriteLine("Marked {0} records.", records.Count);
            }
            return 0;
        }

        static void PublishRecordsNowAsOpenData()
        {
            // load the config
            var configPath = Path.Combine(Environment.CurrentDirectory, "data-gov-uk-publisher-config.json");
            if (!File.Exists(configPath))
                throw new Exception("No data-gov-uk-publisher-config.json file in current directory.");
            string configJson = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<OpenDataPublisherConfig>(configJson);

            var ids = new List<Guid>();
            using (var db = DocumentStore.OpenSession())
            {

                // exclude the ones that have already successfully published (and haven't been updated since)
                // ie those where: never attempted, or the last attempt was unsuccesful, or those that have been updated since the last successful attempt
                var records = db.Query<RecordsWithOpenDataPublicationIndex.Result, RecordsWithOpenDataPublicationIndex>()
                    .Where(x => x.LastPublicationAttemptDate == DateTime.MinValue
                        || x.LastPublicationAttemptDate > x.LastSuccessfulPublicationAttemptDate
                        || x.MetadataDate > x.LastSuccessfulPublicationAttemptDate)
                    .OfType<Record>()
                    .Take(1000)
                    .ToList();
            }

            Console.WriteLine("Publishing {0} records...", ids.Count);

            foreach (var id in ids)
            {
                var ftpClient = new FtpClient(config.FtpUsername, config.FtpPassword);

                using (var db = DocumentStore.OpenSession())
                {
                    new OpenDataRecordPublisher(db, config, ftpClient).PublishRecord(id);
                }
            }

            Console.WriteLine("Published {0} records.", ids.Count);
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

        static void InitDatabase()
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
