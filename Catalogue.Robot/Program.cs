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
using Catalogue.Robot.Injection;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Text;
using Catalogue.Utilities.Time;
using CommandLine;
using CommandLine.Text;
using CsvHelper;
using Newtonsoft.Json;
using Ninject;
using NUnit.Framework.Constraints;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
using Raven.Client;
using Raven.Client.Document;
using Topshelf;

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

    [Verb("mark-as-open-data", HelpText = "Mark records tagged with the specified keyword for publishing as Open Data.")]
    public class MarkAsOpenDataOptions
    {
        [Option(HelpText = "Keyword, e.g. 'vocab.jncc.gov.uk/jncc-category/Some Category'.")]
        public string Keyword { get; set; }

        [Option("id-list-path", HelpText = "A path to the list of Topcat IDs to mark.")]
        public string IdListPath { get; set; }

        [Option("mark-as-corpulent", HelpText = "Mark these records as corpulent - too large to have a data resource.")]
        public bool MarkAsCorpulent { get; set; }
    }

    [Verb("publish-open-data", HelpText = "Publish Open Data records now.")]
    public class PublishOpenDataOptions
    {
        [Option("what-if", Default = false, HelpText = "Don't actually do it.")]
        public bool WhatIf { get; set; }

        [Option("record-id", HelpText = "Just publish one specified record.")]
        public string RecordId { get; set; }

        [Option("metadata-only", Default = false, HelpText = "Don't upload data files; metadata only.")]
        public bool MetadataOnly { get; set; }
    }

    [Verb("delete", HelpText = "Delete all records marked with the metadata-admin Delete tag.")]
    public class DeleteOptions
    {
        [Option("what-if", Default = false, HelpText = "Don't actually do it.")]
        public bool WhatIf { get; set; }
    }

    [Verb("check-resources-exist", HelpText = "Check that referenced data files actually exist.")]
    public class CheckResourcesExistOptions
    {
    }

    [Verb("add-open-data-resources", HelpText = "Add Open Data alternative resources to records via a CSV file.")]
    public class AddOpenDataResourcesOptions
    {
        [Option(Required = true, HelpText = "The path to the CSV file.")]
        public string File { get; set; }

        [Option("remove-existing", Default = false, HelpText = "Remove any existing resources.")]
        public bool RemoveExisting { get; set; }
    }

    class Program
    {
        public static IDocumentStore DocumentStore
        {
            get
            {
                try
                {
                    // DatabaseFactory.Production() returns the RavenDB DocumentStore using the connection
                    // string name specfied in the local web.config or app.config, so this is just what we need
                    // for both production and local dev (where we use the db running in the local dev web app)
                    return DatabaseFactory.Production();
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception("Unable to connect to the Topcat database.", ex);
                }
            }
        }

        static void Main(string[] args)
        {
            //            return Parser.Default.ParseArguments<ImportOptions, MarkAsOpenDataOptions, PublishOpenDataOptions, DeleteOptions, AddOpenDataResourcesOptions>(args).MapResult(
            //                (ImportOptions options) => RunImport(options),
            //                (MarkAsOpenDataOptions options) => RunMarkAsOpenData(options),
            //                (PublishOpenDataOptions options) => RunPublishOpenData(options),
            //                (DeleteOptions options) => RunDelete(options),
            //                (AddOpenDataResourcesOptions options) => RunAddOpenDataResources(options),
            //                (CheckResourcesExistOptions options) => RunCheckResourcesExist(options),
            //                errs => 1);

            HostFactory.Run(x =>
            {
                x.Service<Robot>(s =>
                {
                    s.ConstructUsing(name => CreateRobot());
                    s.WhenStarted(p => p.Start());
                    s.WhenStopped(p => p.Stop());
                });

                x.RunAsLocalSystem();

                string nombre = "Topcat.Robot." + "TODO"; // settings.Environment;
                x.SetDisplayName(nombre);
                x.SetServiceName(nombre);
                x.SetDescription("Description of Robot");
            });
        }

        /// <summary>
        /// Creates an instance with dependecies injected.
        /// </summary>
        public static Robot CreateRobot()
        {
            var kernel = new StandardKernel();

            // register the type bindings we want for injection 
            kernel.Load<MainNinjectModule>();

            return kernel.Get<Robot>();
        }

        static int RunImport(ImportOptions options)
        {
            //InitDatabase();

            using (var db = DocumentStore.OpenSession())
            {
                new ImportHandler(db).Import(options);
            }

            return 0;
        }

        static int RunMarkAsOpenData(MarkAsOpenDataOptions options)
        {
            //InitDatabase();

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

        static void CheckForDuplicateTitles(IDocumentSession db)
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

        static int RunPublishOpenData(PublishOpenDataOptions options)
        {
            //InitDatabase();

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
                if (options.RecordId.IsNotBlank())
                {
                    // publish the specified record
                    ids = new List<Guid> { Guid.Parse(options.RecordId) };
                }
                else
                {
                    // get the records that are pending publication (ie not PublishedSinceLastUpdated)
                    ids = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                        .Where(x => !x.PublishedSinceLastUpdated)
                        .Where(x => x.GeminiValidated) // all open data should be gemini-valid - this is a safety
                        .OfType<Record>() //.Select(r => r.Id) // this doesn't work in RavenDB, and doesn't throw!
                        .Take(1000) // so take 1000 which is enough for one run
                        .ToList() // so materialize the record first
                        .Where(r => !r.Publication.OpenData.Paused) //  .Where(x => !x.PublishingIsPaused) on the server doesn't work on live - thanks, ravenDB
                        .Select(r => r.Id)
                        .ToList();
                }

            }

            Console.WriteLine("Publishing {0} records...", ids.Count);

            if (!options.WhatIf)
            {
                foreach (var id in ids)
                {
                    var ftpClient = new FtpClient(config.FtpUsername, config.FtpPassword);

                    using (var db = DocumentStore.OpenSession())
                    {
                        new OpenDataRecordPublisher(db, config, options.MetadataOnly, ftpClient).PublishRecord(id);
                    }
                }
            }

            Console.WriteLine("Published (or skipped) {0} records.", ids.Count);

            if (options.WhatIf)
            {
                Console.WriteLine("Ran in 'what-if' mode. Nothing was really done.");
            }

            return 1;
        }

        static int RunDelete(DeleteOptions options)
        {
            //InitDatabase();

            string luceneQuery = "Keywords:\"http://vocab.jncc.gov.uk/metadata-admin/Delete\"";

            using (var db = DocumentStore.OpenSession())
            {
                // this loads all the records into memory because i can't figure out how to do it better
                // https://groups.google.com/forum/#!topic/ravendb/ELqhzCs2amY
                int count = db.Advanced.DocumentQuery<Record>("RecordIndex").Where(luceneQuery).ToList().Count;
                Console.WriteLine("Deleting {0} records...", count);
                Console.WriteLine("If this said 128 then it could be more!");
            }

            if (!options.WhatIf)
            {
                DocumentStore.DatabaseCommands.DeleteByIndex("RecordIndex", new IndexQuery { Query = luceneQuery });
            }

            Console.WriteLine("Delete request sent to database.");

            if (options.WhatIf)
            {
               Console.WriteLine("Ran in 'what-if' mode. Nothing was really done.");
            }

            return 1;
        }

        static int RunAddOpenDataResources(AddOpenDataResourcesOptions options)
        {
            //InitDatabase();

            var list = new List<Tuple<string, List<string>>>();

            using (var reader = new StreamReader(options.File))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.HasHeaderRecord = false;

                while (csv.Read())
                {
                    var id = csv.GetField<string>(0);
                    var resourcePaths = csv.CurrentRecord.Skip(1)
                        .Where(value => value.IsNotBlank())
                        .ToList();

                    list.Add(new Tuple<string, List<string>>(id, resourcePaths));
                }
            }

            foreach (var tuple in list)
            {
                using (var db = DocumentStore.OpenSession())
                {
                    var id = Guid.Parse(tuple.Item1);
                    var record = db.Load<Record>(id);

                    var resources = tuple.Item2.Select(r => new Resource { Path = r });

                    if (record.Publication == null)
                        record.Publication = new PublicationInfo();
                    if (record.Publication.OpenData == null)
                        record.Publication.OpenData = new OpenDataPublicationInfo();
                    if (record.Publication.OpenData.Resources == null)
                        record.Publication.OpenData.Resources = new List<Resource>();

                    if (options.RemoveExisting)
                    {
                        record.Publication.OpenData.Resources.Clear();
                    }

                    record.Publication.OpenData.Resources.AddRange(resources);
                    record.Gemini.MetadataDate = Clock.NowUtc; // poke the record to ensure it is publishable

                    db.SaveChanges();
                }
            }

            Console.WriteLine("Added {0} resources to {1} records.", list.Sum(tuple => tuple.Item2.Count), list.Count);

            return 1;
        }

        static int RunCheckResourcesExist(CheckResourcesExistOptions options)
        {
            throw new Exception("TODO");

            //return 1;
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

    }
}
