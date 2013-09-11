using Catalogue.Data;
using Catalogue.Data.Model;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;

namespace Catalogue.Tests.Utility
{
    public class DatabaseTestFixture
    {
        public static IDocumentStore ReusableDocumentStore { get; set; }

        static DatabaseTestFixture()
        {
            // initialise the ResusableDocumentStore once, in this static constructor

            var store = new EmbeddableDocumentStore { RunInMemory = true };
            store.Initialize();

            // seed with test data and wait for indexing
            Seeder.Seed(store);
            Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            RavenUtility.WaitForIndexing(store);
            
            // todo: is it possible to make the database read-only to prevent accidental mutation of test data?

            ReusableDocumentStore = store;
        }

        /// <summary>
        /// A document session open for the lifetime of the test fixture.
        /// Saves repeatedly opening a new session.
        /// </summary>
        protected IDocumentSession Db;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Db = ReusableDocumentStore.OpenSession();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Db.Dispose();
        }
    }
}
