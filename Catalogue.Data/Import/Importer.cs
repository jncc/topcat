using System;
using System.IO;
using Catalogue.Data.Import.Formats;
using Catalogue.Data.Model;
using CsvHelper;
using Raven.Client;

namespace Catalogue.Data.Import
{
    public class Importer<T> where T : IFormat, new()
    {
        readonly IFileSystem fileSystem;
        readonly IDocumentSession db;

        public Importer(IFileSystem fileSystem, IDocumentSession db)
        {
            this.fileSystem = fileSystem;
            this.db = db;
        }

        public void Import(string path)
        {
            using (var reader = fileSystem.OpenReader(path))
            {
                Import(reader);
            }
        }

        public void Import(TextReader reader)
        {
            var csv = new CsvReader(reader);

            var format = Activator.CreateInstance<T>();
            format.Configure(csv.Configuration);

            var records = csv.GetRecords<Record>();

            foreach (var record in records)
            {
                db.Store(record);
            }
        }
     }
}

