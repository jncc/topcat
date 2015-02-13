using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Database.Server;

namespace Catalogue.Data
{
    public class DatabaseFactory
    {
        public static IDocumentStore Production()
        {
            var store = new DocumentStore { ConnectionStringName = "Data" };
            store.Initialize();
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            return store;
        }

        public static IDocumentStore InMemory()
        {
            return new InMemoryDatabaseHelper
            {
                PreInitializationAction = store =>
                {
                    const int port = 8888;
                    store.Configuration.Port = port;
                    NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(port);
                    store.UseEmbeddedHttpServer = true;
                },
                //PostInitializationAction = Seeder.Seed
            }.Create();
        }
    }
}