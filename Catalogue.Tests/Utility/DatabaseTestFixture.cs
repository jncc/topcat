using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using NUnit.Framework;
using Raven.Bundles.Versioning.Data;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Catalogue.Tests.Utility
{
    public class DatabaseTestFixture
    {
        public static IDocumentStore ReusableDocumentStore { get; set; }

        static DatabaseTestFixture()
        {
            // initialise the ResusableDocumentStore once, in this static constructor

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

            // seed with test data and wait for indexing
            Seeder.Seed(store);
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            RavenUtility.WaitForIndexing(store);
            
            // todo: is it possible to make the database read-only to prevent accidental mutation of test data?

            ReusableDocumentStore = store;
        }

        /// <summary>
        /// A document session open for the lifetime of the test fixture.
        /// </summary>
        protected IDocumentSession Db;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {

            Db = ReusableDocumentStore.OpenSession();

            Db.Store(new Raven.Bundles.Versioning.Data.VersioningConfiguration
            {
                Exclude = false,
                Id = "Raven/Versioning/DefaultConfiguration",
                MaxRevisions = 50
            }, "Raven/Versioning/DefaultConfiguration");
            Db.SaveChanges();

        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Db.Dispose();
        }
    }
}
