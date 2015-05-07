using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Moq;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Data.Import
{
    public class Importer<T> where T : IMapping, new()
    {
        readonly IFileSystem fileSystem;
        readonly IRecordService recordService;
        readonly IVocabularyService vocabularyService;

        public bool SkipBadRecords { get; set; }

        public readonly List<RecordServiceResult> Results = new List<RecordServiceResult>();

        public Importer(IFileSystem fileSystem, IRecordService recordService, IVocabularyService vocabularyService)
        {
            this.fileSystem = fileSystem;
            this.recordService = recordService;
            this.vocabularyService = vocabularyService;
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

            int n = 1;
            var keywords = new List<MetadataKeyword>();

            foreach (var record in records)
            {
                var result = recordService.Insert(record);

                if (!result.Success)
                {
                    if (!SkipBadRecords)
                    {
                        throw new Exception(String.Format("Import failed due to validation errors at record {0}: {1}",
                            n, result.Validation.Errors.ToConcatenatedString(e => e.Message, "; ")));
                    }
                }

                n++;
                Results.Add(result);

                keywords.AddRange(result.Record.Gemini.Keywords);
            }

            if (mapping.RequiredVocabularies != null)
            {
                foreach (var vocab in mapping.RequiredVocabularies)
                {
                    try
                    {
                        vocabularyService.Insert(vocab);
                    }
                    catch (InvalidOperationException)
                    {
                        //ignore this - trying to insert an existing vocab.
                    }
                }
            }

            vocabularyService.Import(keywords);
            
        }
    }

    /// <summary>
    /// Helper to conveniently create an importer instance.
    /// </summary>
    public static class Importer
    {
        public static Importer<T> CreateImporter<T>(IDocumentSession db) where T : IMapping, new()
        {
            return new Importer<T>(new FileSystem(), new RecordService(db, new RecordValidator()), new VocabularyService(db, new VocabularyValidator()));
        }
    }


    class when_importing_test_records
    {
        IRecordService recordService;

        [SetUp]
        public void setup()
        {
            var record = new Record
                {
                    Path = @"X:\some\path",
                    Gemini = Library.Blank().With(m => m.Title = "Some new record")
                };
            var result = RecordServiceResult.SuccessfulResult.With(r => r.Record = record);
            recordService = Mock.Of<IRecordService>(rs => rs.Insert(It.IsAny<Record>()) == result);

            string path = @"c:\some\path.csv";
            var fileSystem = Mock.Of<IFileSystem>(fs => fs.OpenReader(path) == new StringReader(testData));

            var importer = new Importer<TestDataMapping>(fileSystem, recordService, Mock.Of<IVocabularyService>());
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
        public IEnumerable<Vocabulary> RequiredVocabularies { get; private set; }

        public TestDataMapping()
        {
            RequiredVocabularies = new List<Vocabulary>();
        }

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

