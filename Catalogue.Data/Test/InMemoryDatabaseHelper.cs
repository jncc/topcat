using System;
using Catalogue.Data.Model;
using Raven.Bundles.Versioning.Data;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Catalogue.Data.Test
{
    public class InMemoryDatabaseHelper
    {
        public Action<EmbeddableDocumentStore> PreInitializationAction { get; set; }
        public Action<IDocumentStore> PostInitializationAction { get; set; }

        public IDocumentStore Create()
        {
            var store = new EmbeddableDocumentStore { RunInMemory = true };
            
            // activate versioning feature bundle
            store.Configuration.Settings.Add("Raven/ActiveBundles", "Versioning");

            if (PreInitializationAction != null)
                PreInitializationAction(store);

            store.Initialize();

            // apparently we need to configure versioning explicity per document type when running in-memory
            using (var db = store.OpenSession())
            {
                db.Store(new VersioningConfiguration
                    {
                        Exclude = false,
                        Id = "Raven/Versioning/Records",
                        MaxRevisions = int.MaxValue

                    });
                db.SaveChanges();
            }

            if (PostInitializationAction != null)
                PostInitializationAction(store);
            
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            RavenUtility.WaitForIndexing(store);

            using (var db = store.OpenSession())
            {
                db.Store(new VersioningConfiguration
                {
                    Exclude = false,
                    Id = "Raven/Versioning/DefaultConfiguration",
                    MaxRevisions = 50
                }, "Raven/Versioning/DefaultConfiguration");

                db.SaveChanges();
            }

            // todo: is it possible to make the database read-only to prevent accidental mutation of test data?
            return store;
        }
    }
}
