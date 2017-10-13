using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Write;
using Newtonsoft.Json;
using Raven.Abstractions.Logging;
using Raven.Client;

namespace Catalogue.Toolbox.Importing
{
    public class ImportHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportHandler));

        readonly IDocumentSession db;

        public ImportHandler(IDocumentSession db)
        {
            this.db = db;
        }

        public void Import(ImportOptions options)
        {
            Logger.Info("Importing records");

            var mapping = InstantiateMapping(options);
            var importer = Importer.CreateImporter(db, mapping);

            importer.SkipBadRecords = options.SkipBadRecords;
            importer.Import(options.File);

            var results = importer.Results;
            foreach (var result in results)
            {
                if (result.Success)
                    Logger.Info("{0} ({1}) imported successfully", result.Record.Id, result.Record.Gemini.Title);
                else
                    Logger.Info("{0} ({1}) skipped", result.Record.Id, result.Record.Gemini.Title);
            }

            string errorFilePath = GetErrorFilePath(options);
            WriteErrorsToFile(results, errorFilePath);

            db.SaveChanges();

            int successes = results.Count(r => r.Success);
            int failures = results.Count(r => !r.Success);

            Logger.Info("Imported {0} records", successes);
            Logger.Info("Skipped {0} records", failures);

            if (failures > 0)
            {
                Logger.Info("See error file for details.");
                Logger.Info(errorFilePath);
            }
        }

        IMapping InstantiateMapping(ImportOptions options)
        {
            var type = typeof(IMapping).Assembly.GetType("Catalogue.Data.Import.Mappings." + options.Mapping);

            if (type == null)
                throw new Exception(String.Format("The import mapping '{0}' couldn't be found or does not exist.", options.Mapping));

            return (IMapping)Activator.CreateInstance(type);
        }

        string GetErrorFilePath(ImportOptions options)
        {
            string directory = Path.GetDirectoryName(options.File);
            string filename = Path.GetFileNameWithoutExtension(options.File);
            return Path.Combine(directory, filename + ".errors.txt");
        }

        void WriteErrorsToFile(List<RecordServiceResult> results, string errorFilePath)
        {
            var errors = results
                .Where(r => !r.Success)
                .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);

            File.WriteAllLines(errorFilePath, errors);
        }
    }
}
