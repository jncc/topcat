using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Write;
using Catalogue.Gemini.Write;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;

namespace Catalogue.Tests.Explicit.Catalogue.Import
{
    internal class import_runner
    {
        [Explicit]
        [Test]
        public void run()
        {
            var store = new DocumentStore();
            store.ParseConnectionString("Url=http://localhost:8888/");
            store.Initialize();

            using (IDocumentSession db = store.OpenSession())
            {
                var importer = new Importer<ActivitiesMapping>(new FileSystem(),
                    new RecordService(db, new RecordValidator(new VocabularyService(db))));
                importer.Import(@"C:\Work\pressures-data\Human_Activities_Metadata_Catalogue.csv");
            }
        }
    }
}