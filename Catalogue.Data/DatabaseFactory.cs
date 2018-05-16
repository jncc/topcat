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
            store.Conventions.MaxNumberOfRequestsPerSession = 1000;
            store.Initialize();
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            return store;
        }

        public static IDocumentStore InMemory(int port = 8888)
        {
            return new InMemoryDatabaseHelper
            {
                PreInitializationAction = store =>
                {
                    store.Configuration.Port = port;
                    NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(port);
                    store.UseEmbeddedHttpServer = true;
                    store.Configuration.Storage.Voron.AllowOn32Bits = true;
                },
                PostInitializationAction = Seeder.Seed
            }.Create();
        }
    }
}