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
    public class Importer
    {
        /// <summary>
        /// Helper to conveniently create an importer instance.
        /// </summary>
        public static Importer CreateImporter(IDocumentSession db, IMapping mapping)
        {
            return new Importer(mapping, new FileSystem(), new RecordService(db, new RecordValidator()), new VocabularyService(db, new VocabularyValidator()), new UserInfo());
        }

        readonly IMapping mapping;
        readonly IFileSystem fileSystem;
        readonly IRecordService recordService;
        readonly IVocabularyService vocabularyService;
        private readonly UserInfo userInfo;

        public bool SkipBadRecords { get; set; }

        public readonly List<RecordServiceResult> Results = new List<RecordServiceResult>();

        public Importer(IMapping mapping, IFileSystem fileSystem, IRecordService recordService, IVocabularyService vocabularyService, UserInfo userInfo)
        {
            this.mapping = mapping;
            this.fileSystem = fileSystem;
            this.recordService = recordService;
            this.vocabularyService = vocabularyService;
            this.userInfo = userInfo;
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

            mapping.Apply(csv.Configuration);

            var records = csv.GetRecords<Record>();

            int n = 1;
            var keywords = new List<MetadataKeyword>();

            try
            {
                foreach (var record in records)
                {
                    var result = recordService.Insert(record, userInfo);

                    if (!result.Success)
                    {
                        if (!SkipBadRecords)
                        {
                            throw new ImportException(String.Format("Import failed due to validation errors at record {0}: {1}",
                                n, result.Validation.Errors.ToConcatenatedString(e => e.Message, "; ")));
                        }
                    }

                    n++;
                    Results.Add(result);

                    keywords.AddRange(result.Record.Gemini.Keywords);
                }
            }
            catch (CsvHelperException ex)
            {
                string info = (string) ex.Data["CsvHelper"];
                throw new ImportException("CsvHelper exception: " + info, ex);
            }

            // import new vocabs and keywords
            foreach (var vocab in mapping.RequiredVocabularies)
            {
                // if vocab already exists, Insert returns a VocabularyServiceResult with Success=false, which we just ignore
                vocabularyService.Insert(vocab);
            }
            vocabularyService.AddKeywordsToExistingControlledVocabs(keywords);
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
            recordService = Mock.Of<IRecordService>(rs => rs.Insert(It.IsAny<Record>(), It.IsAny<UserInfo>()) == result);

            string path = @"c:\some\path.csv";
            var fileSystem = Mock.Of<IFileSystem>(fs => fs.OpenReader(path) == new StringReader(testData));

            var importer = new Importer(new TestDataMapping(), fileSystem, recordService, Mock.Of<IVocabularyService>(), new UserInfo());
            importer.Import(path);
        }

        [Test]
        public void should_import_both_records()
        {
            Mock.Get(recordService).Verify(s => s.Insert(It.IsAny<Record>(), It.IsAny<UserInfo>()), Times.Exactly(2));
        }

        [Test]
        public void should_import_gemini_object()
        {
            // make sure that the importer is filling in the gemini object as well as the top-level field(s)
            Mock.Get(recordService).Verify(s => s.Insert(It.Is((Record r) => r.Gemini.Abstract == "This is the abstract"), It.IsAny<UserInfo>()));
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

