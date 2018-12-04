using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Toolbox.Importing;
using CommandLine;
using log4net;
using log4net.Config;
using Raven.Client.Documents;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Operations;

namespace Catalogue.Toolbox
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

    [Verb("delete", HelpText = "Delete all records marked with the metadata-admin Delete tag.")]
    public class DeleteOptions
    {
        [Option("what-if", Default = false, HelpText = "Don't actually do it.")]
        public bool WhatIf { get; set; }
    }

    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        static int Main(string[] args)
        {
            GlobalContext.Properties["LogFileName"] = ConfigurationManager.AppSettings["LogFilePath"];
            XmlConfigurator.Configure();

            return Parser.Default.ParseArguments<ImportOptions, DeleteOptions>(args).MapResult(
                (ImportOptions options) => RunImport(options),
                (DeleteOptions options) => RunDelete(options),
                errs => 1);
        }

        public static IDocumentStore DocumentStore { get; private set; }

        static int RunImport(ImportOptions options)
        {
            InitDatabase();

            using (var db = DocumentStore.OpenSession())
            {
                new ImportHandler(db).Import(options);
            }

            return 0;
        }

        static int RunDelete(DeleteOptions options)
        {
            InitDatabase();
            
            string luceneQuery = "Keywords:\"http://vocab.jncc.gov.uk/metadata-admin/Delete\"";
            
            using (var db = DocumentStore.OpenSession())
            {
                // this loads all the records into memory because i can't figure out how to do it better
                // https://groups.google.com/forum/#!topic/ravendb/ELqhzCs2amY

                var records = db.Advanced.DocumentQuery<Record>("RecordIndex").WhereLucene("Keywords", luceneQuery).ToList();
                Logger.Info($"Deleting {records.Count} records:");
                foreach (var record in records)
                    Logger.Info($"{record.Id} ({record.Gemini.Title})");
                Logger.Info("If this said 128 then it could be more!");

                if (!options.WhatIf)
                {
                    foreach (var record in records)
                    {
                        var command = new DeleteDocumentCommand(record.Id, null);
                        db.Advanced.RequestExecutor.Execute(command, db.Advanced.Context);
                    }
                }

                Logger.Info("Delete requests sent to database.");
            }

            if (options.WhatIf)
            {
                Logger.Info("Ran in 'what-if' mode. Nothing was really done.");
            }

            return 1;
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
