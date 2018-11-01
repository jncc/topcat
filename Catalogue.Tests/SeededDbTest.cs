using System;
using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Utilities.Time;
using NUnit.Framework;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.TestDriver;

namespace Catalogue.Tests
{
    /// <summary>
    /// Extend this class for tests that run against seeded records
    /// use the Db references
    /// </summary>
    public class SeededDbTest : DatabaseTestFixture
    {
        public static IDocumentStore ReusableDocumentStore { get; set; }

        /// <summary>
        /// A document session open for the lifetime of the test fixture.
        /// </summary>
        protected IDocumentSession Db;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var store = GetDocumentStore();
            store.Initialize();
            Seeder.Seed(store);
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            WaitForIndexing(store);
            ReusableDocumentStore = store;
            Db = ReusableDocumentStore.OpenSession();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Db.Dispose();
            ReusableDocumentStore.Dispose();
            Dispose();
        }
    }
}
