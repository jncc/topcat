using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Write;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;

namespace Catalogue.Tests.Explicit.Catalogue.Import
{
    internal class import_runner
    {
        [Explicit]
        [Test]
        public void RunActivitiesImport()
        {
            var store = new DocumentStore();
            store.ParseConnectionString("Url=http://localhost:8888/");
            store.Initialize();

            using (IDocumentSession db = store.OpenSession())
            {
                var importer = Importer.CreateImporter(db, new ActivitiesMapping());
                importer.Import(@"C:\Work\pressures-data\Human_Activities_Metadata_Catalogue.csv");
            }
        }
    }
}