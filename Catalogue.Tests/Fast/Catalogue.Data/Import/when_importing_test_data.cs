using System.IO;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;
using Moq;
using NUnit.Framework;

namespace Catalogue.Tests.Fast.Catalogue.Data.Import
{
    class when_importing_test_records
    {
        IRecordService recordService;

        [SetUp]
        public void setup()
        {
            recordService = Mock.Of<IRecordService>();

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
@"Abstract,Notes,Blah
This is the abstract,These are the notes
Another abstract,Some more notes";

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
