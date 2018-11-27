using System;
using System.Collections.Generic;
using System.IO;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using CsvHelper.Configuration;
using Moq;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Import
{
    public class when_importing_test_records
    {
        IRecordService recordService;

        [SetUp]
        public void setup()
        {
            var record = new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m => m.Title = "Some new record"),
                Footer = new Footer()
            };
            var result = RecordServiceResult.SuccessfulResult.With(r => r.Record = record );
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

    public class TestDataMapping : IReaderMapping
    {
        public IEnumerable<Vocabulary> RequiredVocabularies { get; private set; }

        public TestDataMapping()
        {
            RequiredVocabularies = new List<Vocabulary>();
        }

        public void Apply(IReaderConfiguration config)
        {
            // see http://joshclose.github.io/CsvHelper/

            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<MetadataMap>();
        }

        public sealed class RecordMap : ClassMap<Record>
        {
            public RecordMap()
            {
                Map(m => m.Path);
                Map(m => m.Notes);
                References<MetadataMap>(m => m.Gemini);
            }
        }

        public sealed class MetadataMap : ClassMap<Metadata>
        {
            public MetadataMap()
            {
                Map(m => m.Abstract);
            }
        }
    }
}
