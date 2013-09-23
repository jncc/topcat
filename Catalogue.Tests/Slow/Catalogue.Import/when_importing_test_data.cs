using System.IO;
using System.Linq;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Formats;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Tests.Utility;
using CsvHelper.Configuration;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;

namespace Catalogue.Tests.Slow.Catalogue.Import
{
    class when_importing_test_data
    {
        IDocumentStore store;
        IDocumentSession db;

        [SetUp]
        public void setup()
        {
            store = new EmbeddableDocumentStore { RunInMemory = true };
            store.Initialize();

            string path = @"c:\some\path.csv";
            var fileSystem = Mock.Of<IFileSystem>(fs => fs.OpenReader(path) == new StringReader(testData));

            using (var x = store.OpenSession())
            {
                var importer = new Importer<TestDataFormat>(fileSystem, x);
                importer.Import(path);

                x.SaveChanges();
            }

            RavenUtility.WaitForIndexing(store);
            db = store.OpenSession();
        }

        [Test]
        public void should_import_all_records()
        {
            db.Query<Record>().Count().Should().Be(2);
        }

        [Test]
        public void should_import_gemini_object()
        {
            // make sure that the importer is filling in the gemini object as well as the top-level field(s)
            var record = db.Query<Record>().Single(r => r.Notes == "These are the notes");
            record.Gemini.Abstract.Should().Be("This is the abstract");
        }

        string testData =
@"Abstract,Notes,Blah
This is the abstract,These are the notes
Another abstract,Some more notes";

        [TearDown]
        public void tear_down()
        {
            if (db != null)
                db.Dispose();
        }

    }

    public class TestDataFormat : IFormat
    {
        public void Configure(CsvConfiguration config)
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
