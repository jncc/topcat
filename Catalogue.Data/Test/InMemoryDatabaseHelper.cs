using System;
using System.Configuration;
using Catalogue.Data.Model;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.TestDriver;

namespace Catalogue.Data.Test
{
    public class InMemoryDatabaseHelper : RavenTestDriver
    {
        public InMemoryDatabaseHelper(TestServerOptions options)
        {
            ConfigureServer(options);
        }

        public IDocumentStore Create(Action<IDocumentStore> postInitializationAction = null)
        {
            var store = GetDocumentStore();

            store.Initialize();

            if (postInitializationAction != null)
                postInitializationAction(store);            

            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            
            WaitForIndexing(store);

            return store;
        }
    }
}
