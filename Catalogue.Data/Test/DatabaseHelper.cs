using System;
using Catalogue.Data.Model;
using Raven.Bundles.Versioning.Data;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Catalogue.Data.Test
{
    public static class DatabaseHelper
    {
        public static IDocumentStore CreateInMemoryStore(Action<IDocumentStore> populate)
        {
            var store = new EmbeddableDocumentStore { RunInMemory = true };
            
            // activate versioning feature bundle
            store.Configuration.Settings.Add("Raven/ActiveBundles", "Versioning");
            
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

            populate(store);
            
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            RavenUtility.WaitForIndexing(store);

            using (var db = store.OpenSession())
            {
                db.Store(new Raven.Bundles.Versioning.Data.VersioningConfiguration
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
