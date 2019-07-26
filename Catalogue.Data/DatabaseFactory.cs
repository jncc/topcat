using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using System.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.TestDriver;

namespace Catalogue.Data
{
    public class DatabaseFactory
    {
        public static IDocumentStore Production()
        {
            var store = new DocumentStore
            {
                Urls = new[] { ConfigurationManager.AppSettings["RavenDbUrls"] },
                Database = ConfigurationManager.AppSettings["RavenDbDatabase"]
            };
            store.Conventions.MaxNumberOfRequestsPerSession = 100;
            store.Initialize();
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);

            return store;
        }

        public static IDocumentStore InMemory()
        {
            var helper = new InMemoryDatabaseHelper(new TestServerOptions
            {
                FrameworkVersion = ConfigurationManager.AppSettings["RavenDbFrameworkVersion"],
                ServerUrl = ConfigurationManager.AppSettings["RavenDbUrls"]
            });
            return helper.Create(Seeder.Seed);
        }
    }
}