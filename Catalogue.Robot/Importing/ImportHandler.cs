using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Write;
using Newtonsoft.Json;
using Raven.Client;

namespace Catalogue.Robot.Importing
{
    public class ImportHandler
    {
        readonly IDocumentSession db;

        public ImportHandler(IDocumentSession db)
        {
            this.db = db;
        }

        public void Import(ImportOptions options)
        {
            var mapping = InstantiateMapping(options);
            var importer = Importer.CreateImporter(db, mapping);

            importer.SkipBadRecords = options.SkipBadRecords;
            importer.Import(options.File);

            string errorFilePath = GetErrorFilePath(options);
            WriteErrorsToFile(importer.Results, errorFilePath);

            db.SaveChanges();

            int successes = importer.Results.Count(r => r.Success);
            int failures = importer.Results.Count(r => !r.Success);

            Console.WriteLine("Imported {0} records.", successes);
            Console.WriteLine("Skipped {0} records.", failures);

            if (failures > 0)
            {
                Console.WriteLine("See error file for details.");
                Console.WriteLine(errorFilePath);
            }
        }

        IMapping InstantiateMapping(ImportOptions options)
        {
            var type = typeof(IMapping).Assembly.GetType("Catalogue.Data.Import.Mappings." + options.Mapping);

            if (type  == null)
                throw new Exception(String.Format("The import mapping '{0}' couldn't be found or does not exist.", options.Mapping));

            return (IMapping) Activator.CreateInstance(type);
        }

        string GetErrorFilePath(ImportOptions options)
        {
            string directory = Path.GetDirectoryName(options.File);
            string filename = Path.GetFileNameWithoutExtension(options.File);
            return Path.Combine(directory, filename + ".errors.txt");
        }

        void WriteErrorsToFile(List<RecordServiceResult> results , string errorFilePath)
        {
            var errors = results
                .Where(r => !r.Success)
                .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);

            File.WriteAllLines(errorFilePath, errors);
        }
    }
}
