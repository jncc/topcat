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
        public enum DatabaseConnectionType
        {
            Proper,
            InMemory,
            ReUseable
        }


        public static IDocumentStore Create(DatabaseConnectionType databaseConnectionType)
        {
            var db = CreateDatabase(databaseConnectionType);
             CreateIndices(db);
             var index = db.DatabaseCommands.GetIndex("KeywordsIndex");
            return db;
        }

        private static IDocumentStore CreateDatabase(DatabaseConnectionType databaseConnectionType)
        {
            if (databaseConnectionType == DatabaseConnectionType.InMemory)
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
                    PostInitializationAction = Seeder.Seed
                }.Create();
            }
            else if (databaseConnectionType == DatabaseConnectionType.Proper)
            {
                var documentStore = new DocumentStore {ConnectionStringName = "Data"};
                documentStore.Initialize();
                return documentStore;
            }
            else // (databaseConnectionType == DatabaseConnectionType.ReUseable)
            {
                return new InMemoryDatabaseHelper { PostInitializationAction = Seeder.Seed }.Create();               
            }
 
        }

        private static IDocumentStore CreateIndices(IDocumentStore documentStore)
        {
            
            var keywordIndex = new KeywordsIndex();
            documentStore.ExecuteIndex(keywordIndex);
            IndexCreation.CreateIndexes(typeof(Record).Assembly, documentStore);
            return documentStore;
        }
    }
}