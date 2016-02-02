using System;
using Catalogue.Data;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using Catalogue.Utilities.Time;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Tests
{
    /// <summary>
    /// Extend this class for tests that require an instance of in-memory database
    /// use the Db references
    /// </summary>
    public class DatabaseTestFixture
    {
        public static IDocumentStore ReusableDocumentStore { get; set; }

        static DatabaseTestFixture()
        {
            // ensure deterministic date for metadata date, which is always set to Clock.NowUtc when records are inserted
            Clock.CurrentUtcDateTimeGetter = () => new DateTime(2015, 1, 1, 12, 0, 0);

            // initialise the ResusableDocumentStore once, in this static constructor
            ReusableDocumentStore = DatabaseFactory.InMemory(); 
        }

        /// <summary>
        /// A document session open for the lifetime of the test fixture.
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


    public class AsyncDatabaseTestFixture
    {
        public static IDocumentStore ReusableDocumentStore { get; set; }

        static AsyncDatabaseTestFixture()  
        {
            // initialise the ResusableDocumentStore once, in this static constructor
            ReusableDocumentStore = DatabaseFactory.InMemory();
        }

        /// <summary>
        /// An Async document session open for the lifetime of the test fixture.
        /// </summary>
        protected IAsyncDocumentSession Db;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Db = ReusableDocumentStore.OpenAsyncSession();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Db.Dispose();
        }
    }
}
