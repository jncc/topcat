using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Import;
using Catalogue.Import.Formats;
using Catalogue.Import.Utilities;
using Catalogue.Tests.Utility;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace Catalogue.Tests.Slow.Catalogue.Import
{
    class importer_tests
    {
        [Test]
        public void should_import_data()
        {
            var store = new EmbeddableDocumentStore { RunInMemory = true };
            store.Initialize();

            string path = @"c:\some\path.csv";
            
            var fileSystem = Mock.Of<IFileSystem>(fs => fs.OpenReader(path) == new StringReader(data));

            var importer = new Importer<DefaultFormat>(fileSystem, store);
            importer.Import(path);

            RavenUtility.WaitForIndexing(store);
            using (var db = store.OpenSession())
            {
                db.Query<Record>().Count().Should().Be(2);
            }
        }

        string data =
@"Abstract,Notes,Blah
This is the abstract,These are the notes
Another abstract,Some more notes";

    }
}
