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
        public Action<IDocumentStore> PreInitializationAction { get; set; }
        public Action<IDocumentStore> PostInitializationAction { get; set; }

        public IDocumentStore Create()
        {
            ConfigureServer(new TestServerOptions
            {
                FrameworkVersion = "2.1.5",
                ServerUrl = ConfigurationManager.AppSettings["RavenDbUrls"]
            });
            var store = GetDocumentStore();
            
            // raven4
            //            var dsl = new DocumentSessionListeners
            //            {
            //                QueryListeners = new IDocumentQueryListener[] { new NoStaleQueriesListener() }
            //            };
            //            store.SetListeners(dsl);

            if (PreInitializationAction != null)
                PreInitializationAction(store);

            store.Initialize();

            if (PostInitializationAction != null)
                PostInitializationAction(store);            

            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);

            // raven4
            //RavenUtility.WaitForIndexing(store);
            WaitForIndexing(store);

            // todo: is it possible to make the database read-only to prevent accidental mutation of test data?
            return store;
        }
    }
}
