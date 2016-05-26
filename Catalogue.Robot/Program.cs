using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Catalogue.Data;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Gemini.Model;
using Catalogue.Robot.Importing;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Text;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using NUnit.Framework.Constraints;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
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

    [Verb("mark", HelpText = "Mark records tagged with the specified keyword for publishing.")]
    public class MarkOptions
    {
        [Option(HelpText = "Keyword, e.g. 'vocab.jncc.gov.uk/jncc-category/Some Category'.")]
        public string Keyword { get; set; }

        [Option("id-list-path", HelpText = "A path to the list of Topcat IDs to mark.")]
        public string IdListPath { get; set; }

        [Option("mark-as-corpulent", HelpText = "Mark these records as corpulent - too large to have a data resource.")]
        public bool MarkAsCorpulent { get; set; }
    }

    [Verb("publish", HelpText = "Publish now as Open Data.")]
    public class PublishOptions
    {
        [Option("what-if", Default = false, HelpText = "Don't actually do it.")]
        public bool WhatIf { get; set; }
    }

    [Verb("delete", HelpText = "Delete all records marked with the metadata-admin Delete tag")]
    public class DeleteOptions
    {
        [Option("what-if", Default = false, HelpText = "Don't actually do it.")]
        public bool WhatIf { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ImportOptions, MarkOptions, PublishOptions, DeleteOptions>(args).MapResult(
                (ImportOptions options) => RunImportAndReturnExitCode(options),
                (MarkOptions options) => RunMarkAndReturnExitCode(options),
                (PublishOptions options) => RunPublishAndReturnExitCode(options),
                (DeleteOptions options) => RunDeleteAndReturnExitCode(options),
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

        static int RunMarkAndReturnExitCode(MarkOptions options)
        {
            InitDatabase();

            using (var db = DocumentStore.OpenSession())
            {
                List<Record> records;
                if (options.Keyword.IsNotBlank())
                {
                    Console.WriteLine("Marking records tagged '{0}'", options.Keyword);

                    records = GetRecords(db, options.Keyword);
                }
                else if (options.IdListPath.IsNotBlank())
                {
                    var ids = File.ReadAllLines(options.IdListPath)
                        .Where(line => line.IsNotBlank() && !line.StartsWith("#"))
                        .Select(line => "records/" + line.Trim());

                    records = db.Load<Record>(ids).ToList();
                }
                else
                {
                    throw new Exception("Must specify either --keyword or --id-list-path.");
                }

                int count = 0;
                Console.WriteLine("Found {0} records tagged '{1}'.", records.Count, options.Keyword);

                foreach (var record in records)
                {
                    if (record.Publication == null)
                        record.Publication = new PublicationInfo();

                    if (record.Publication.OpenData == null)
                    {
                        record.Publication.OpenData = new OpenDataPublicationInfo();
                        count++;
                    }

                    if (options.MarkAsCorpulent)
                    {
                        record.Gemini.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/metadata-admin", Value = "Corpulent" });
                    }
                }

                db.SaveChanges();
                Console.WriteLine("Marked {0} records.", count);
            }
            return 0;
        }

        private static void CheckForDuplicateTitles(IDocumentSession db)
        {
            var results = db.Query<RecordsWithDuplicateTitleCheckerIndex.Result, RecordsWithDuplicateTitleCheckerIndex>()
                .Where(x => x.Count > 1)
                .Take(100)
                .ToList();

            if (results.Any())
            {
                results.Select(r => new { r.Title, r.Count }).ForEach(Console.WriteLine);
                throw new Exception("There are records with duplicate titles in Topcat. THey need to be removed before Open Data publishing can resume.");
            }
        }

        static int RunPublishAndReturnExitCode(PublishOptions options)
        {
            InitDatabase();

            // load the config
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

            Console.WriteLine("Publishing to '{0}'", config.FtpRootUrl);

            var ids = new List<Guid>();
            using (var db = DocumentStore.OpenSession())
            {
                // there are currently duplicates in what looks like the original seabed habitat map collection
                //CheckForDuplicateTitles(db);

                // get the records for publishing
                ids = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                    .Where(x => !x.PublishedSinceLastUpdated)
                    .Where(x => x.GeminiValidated) // all open data should be gemini-valid - this is a safety don't try to publish 
                    .OfType<Record>()
//                  .Select(r => r.Id) // this doesn't work in RavenDB, and doesn't throw
                    .Take(1000)
                    .ToList() // so materialize the record first
                    .Select(r => r.Id)
                    .ToList();
            }

            Console.WriteLine("Publishing {0} records...", ids.Count);

            if (!options.WhatIf)
            {
                foreach (var id in ids)
                {
                    var ftpClient = new FtpClient(config.FtpUsername, config.FtpPassword);

                    using (var db = DocumentStore.OpenSession())
                    {
                        new OpenDataRecordPublisher(db, config, ftpClient).PublishRecord(id);
                    }
                }
            }

            Console.WriteLine("Published (or skipped) {0} records.", ids.Count);

            return 1;
        }

        static int RunDeleteAndReturnExitCode(DeleteOptions options)
        {
            InitDatabase();

            string luceneQuery = "Keywords:\"http://vocab.jncc.gov.uk/metadata-admin/Delete\"";

            using (var db = DocumentStore.OpenSession())
            {
                // this loads all the records into memory because i can't figure out how to do it better
                // https://groups.google.com/forum/#!topic/ravendb/ELqhzCs2amY
                int count = db.Advanced.DocumentQuery<Record>("RecordIndex").Where(luceneQuery).ToList().Count;
                Console.WriteLine("Deleting {0} records...", count);
            }

            if (!options.WhatIf)
            {
                DocumentStore.DatabaseCommands.DeleteByIndex("RecordIndex", new IndexQuery { Query = luceneQuery });
            }

            Console.WriteLine("Delete request sent to database.");

            return 1;
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
