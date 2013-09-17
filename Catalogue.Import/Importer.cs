using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Catalogue.Data.Model;
using Catalogue.Import.Formats;
using Catalogue.Import.Utilities;
using CsvHelper;
using CsvHelper.Configuration;
using Raven.Client;

namespace Catalogue.Import
{
    public class Importer<T> where T : IFormat, new()
    {
        readonly IFileSystem fileSystem;
        readonly IDocumentStore store;

        public Importer(IFileSystem fileSystem, IDocumentStore store)
        {
            this.fileSystem = fileSystem;
            this.store = store;
        }

 
        public void Import(string path)
        {
            using (var reader = fileSystem.OpenReader(path))
            {
                var csv = new CsvReader(reader);

                var format = Activator.CreateInstance<T>();
                format.Configure(csv.Configuration);

                var records = csv.GetRecords<Record>();

                using (var db = store.OpenSession())
                {
                    foreach (var record in records)
                    {
                        db.Store(record);
                    }

                    db.SaveChanges();
                }

            }
        }
    }

}
