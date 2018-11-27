using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using System.Configuration;
//using Raven.Client.Document;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
//using Raven.Client.Indexes;
//using Raven.Database.Server;

namespace Catalogue.Data
{
    public class DatabaseFactory
    {
        public static IDocumentStore Production()
        {
            //            var store = new DocumentStore { ConnectionStringName = "Data" };
            //            store.Conventions.MaxNumberOfRequestsPerSession = 1000;
            //            store.Initialize();
            //            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            //            return store;

            var store = new DocumentStore
            {
                Urls = new[] { ConfigurationManager.AppSettings["RavenDbUrls"] },
                Database = ConfigurationManager.AppSettings["RavenDbDatabase"]
            };
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
                    //raven4
                    //store.GetConfiguration.Port = port;
                    //NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(port);
                    //store.UseEmbeddedHttpServer = true;
                    //store.Configuration.Storage.Voron.AllowOn32Bits = true;
                },
                PostInitializationAction = Seeder.Seed
            }.Create();
        }
    }
}