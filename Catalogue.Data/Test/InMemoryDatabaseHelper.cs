using System;
using Catalogue.Data.Model;
//using Raven.Bundles.Versioning.Data;
using Raven.Client;
//using Raven.Client.Document;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Embedded;
//using Raven.Client.Indexes;
//using Raven.Client.Listeners;
using Catalogue.Data.Seed;
using Catalogue.Data.Indexes;

namespace Catalogue.Data.Test
{
    public class InMemoryDatabaseHelper
    {
        public Action<IDocumentStore> PreInitializationAction { get; set; }
        public Action<IDocumentStore> PostInitializationAction { get; set; }

        public IDocumentStore Create()
        {
            EmbeddedServer.Instance.StartServer(new ServerOptions
            {
                FrameworkVersion = "2.1.5",
                ServerUrl = "http://localhost:8080"
            });
            var store = EmbeddedServer.Instance.GetDocumentStore("topcat");
            //
            //            var dsl = new DocumentSessionListeners
            //            {
            //                QueryListeners = new IDocumentQueryListener[] { new NoStaleQueriesListener() }
            //            };
            //            store.SetListeners(dsl);
            //
            //            // activate versioning bundle
            //            store.Configuration.Settings.Add("Raven/ActiveBundles", "Versioning");
            //
            if (PreInitializationAction != null)
                PreInitializationAction(store);

            store.Initialize();

            //            // configure versioning bundle
            //using (var db = store.OpenSession())
            //{
            //                db.Store(new VersioningConfiguration
            //                    {
            //                        Exclude = false,
            //                        Id = "Raven/Versioning/DefaultConfiguration",
            //                        MaxRevisions = 50
            //                    });
            //
            //                // apparently we need to configure versioning explicity per document type when running in-memory
            //                db.Store(new VersioningConfiguration
            //                    {
            //                        Exclude = false,
            //                        Id = "Raven/Versioning/Records",
            //                        MaxRevisions = int.MaxValue
            //                    });
            //
            //                db.SaveChanges();
            //            }
            //
            //            // guid keys are problematic for raven's document versioning bundle
            //            // because the key string now contains the normal key plus the version so
            //            // we have to sneak a hack during the hydration of a Record object 
            //            // https://groups.google.com/d/msg/ravendb/iawGaXdzwZA/ty8n2-ylHFsJ
            //            store.Conventions.FindIdValuePartForValueTypeConversion = (entity, id) =>
            //            {
            //                var parts = id.Split('/');
            //                var guid = parts[1];
            //
            //                if (entity is Record && parts.Length == 4)
            //                    ((Record)entity).Revision = int.Parse(parts[3]);
            //
            //                return guid;
            //            };
            //
            //if (PostInitializationAction != null)
            //    PostInitializationAction(store);

            Seeder.Seed(store);
            

            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            RavenUtility.WaitForIndexing(store);

            // todo: is it possible to make the database read-only to prevent accidental mutation of test data?
            return store;
        }
    }
}
