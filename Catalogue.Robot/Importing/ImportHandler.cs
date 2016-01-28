using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
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
            var mapping = typeof(IMapping).Assembly.GetType("Catalogue.Data.Import.Mappings." + options.Mapping);
            if (mapping  == null)
                throw new Exception(String.Format("The import mapping '{0}' couldn't be found or does not exist.", options.Mapping));

            var importer = Importer.CreateImporter(db, (IMapping) Activator.CreateInstance(mapping));

            importer.SkipBadRecords = options.SkipBadRecords;
            importer.Import(options.File);
            db.SaveChanges();

            Console.WriteLine("Imported {0} records.", importer.Results.Count(r => r.Success));
            Console.WriteLine("Skipped {0} records.", importer.Results.Count(r => !r.Success));
        }
    }
}
