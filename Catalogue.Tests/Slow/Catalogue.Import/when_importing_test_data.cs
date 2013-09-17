using System.IO;
using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Import;
using Catalogue.Import.Formats;
using Catalogue.Import.Utilities;
using Catalogue.Tests.Utility;
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

            var importer = new Importer<DefaultFormat>(fileSystem, store);
            importer.Import(path);

            RavenUtility.WaitForIndexing(store);
            db = store.OpenSession();
        }

        [TearDown]
        public void test_down()
        {
            db.Dispose();
        }

        [Test]
        public void should_import_all_records_in_test_data()
        {
            db.Query<Record>().Count().Should().Be(2);
        }

        [Test]
        public void should_import_gemini_component()
        {
            var record = db.Query<Record>().Single(r => r.Notes == "These are the notes");
            record.Gemini.Abstract.Should().Be("This is the abstract");
        }

        string testData =
@"Abstract,Notes,Blah
This is the abstract,These are the notes
Another abstract,Some more notes";

    }
}
