using System;
using System.Collections.Generic;
using System.IO;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using CsvHelper;
using CsvHelper.Configuration;
using Moq;
using NUnit.Framework;

namespace Catalogue.Data.Import
{
    public class Importer<T> where T : IMapping, new()
    {
        readonly IFileSystem fileSystem;
        readonly IRecordService recordService;

        public bool SkipBadRecords { get; set; }
        public readonly List<RecordValidationResult> Failures = new List<RecordValidationResult>();

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
                var result = recordService.Insert(record);

                if (!result.Success)
                {
                    Failures.Add(result);

                    if (!SkipBadRecords)
                        throw new Exception("Import failed. " + result.Message);
                }
            }
        }
    }


    class when_importing_test_records
    {
        IRecordService recordService;

        [SetUp]
        public void setup()
        {
            recordService = Mock.Of<IRecordService>(rs => rs.Insert(It.IsAny<Record>()).Success == true);

            string path = @"c:\some\path.csv";
            var fileSystem = Mock.Of<IFileSystem>(fs => fs.OpenReader(path) == new StringReader(testData));

            var importer = new Importer<TestDataMapping>(fileSystem, recordService);
            importer.Import(path);
        }

        [Test]
        public void should_import_both_records()
        {
            Mock.Get(recordService).Verify(s => s.Insert(It.IsAny<Record>()), Times.Exactly(2));
        }

        [Test]
        public void should_import_gemini_object()
        {
            // make sure that the importer is filling in the gemini object as well as the top-level field(s)
            Mock.Get(recordService).Verify(s => s.Insert(It.Is((Record r) => r.Gemini.Abstract == "This is the abstract")));
        }

        string testData =
@"Abstract,Notes,Path,Blah
This is the abstract,These are the notes,Z:\some\location
Another abstract,Some more notes,file:///z/some/location";

    }

    public class TestDataMapping : IMapping
    {
        public void Apply(CsvConfiguration config)
        {
            // see http://joshclose.github.io/CsvHelper/

            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<MetadataMap>();
        }

        public class RecordMap : CsvClassMap<Record>
        {
            public override void CreateMap()
            {
                this.Map(m => m.Path);
                this.Map(m => m.Notes);
                this.References<MetadataMap>(m => m.Gemini);
            }
        }

        public class MetadataMap : CsvClassMap<Metadata>
        {
            public override void CreateMap()
            {
                this.Map(m => m.Abstract);
            }
        }
    }


}

