using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using NUnit.Framework;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Catalogue.Tests.Explicit.Catalogue.Import
{
    internal class import_runner
    {
        [Explicit]
        [Test]
        public void RunActivitiesImport()
        {
            var store = new DocumentStore
            {
                Urls = new[] { "http://localhost:8080/" }
            };
            store.Initialize();

            using (IDocumentSession db = store.OpenSession())
            {
                var importer = Importer.CreateImporter(db, new ActivitiesMapping());
                importer.Import(@"C:\Work\pressures-data\Human_Activities_Metadata_Catalogue.csv");
            }
        }
    }
}