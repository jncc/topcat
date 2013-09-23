using System;
using System.IO;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using CsvHelper;

namespace Catalogue.Data.Import
{
    public class Importer<T> where T : IMapping, new()
    {
        readonly IFileSystem fileSystem;
        readonly IRecordService recordService;

        public Importer(IFileSystem fileSystem, IRecordService recordService)
        {
            this.fileSystem = fileSystem;
            this.recordService = recordService;
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

            var mapping = Activator.CreateInstance<T>();
            mapping.Apply(csv.Configuration);

            var records = csv.GetRecords<Record>();

            foreach (var record in records)
            {
                recordService.Insert(record);
            }
        }
     }
}

