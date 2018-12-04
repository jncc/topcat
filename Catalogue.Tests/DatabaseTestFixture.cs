using Catalogue.Data;
using Catalogue.Utilities.Time;
using NUnit.Framework;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using System;
using Catalogue.Data.Test;

namespace Catalogue.Tests
{
    /// <summary>
    /// Extend this class for tests that require an instance of in-memory database
    /// </summary>
    public class DatabaseTestFixture : RavenTestDriver
    {
        protected static InMemoryDatabaseHelper DbHelper { get; set; }
        protected static IDocumentStore ReusableDocumentStore { get; set; }

        static DatabaseTestFixture()
        {
            // ensure deterministic date for metadata date, which is always set to Clock.NowUtc when records are inserted
            Clock.CurrentUtcDateTimeGetter = () => new DateTime(2015, 1, 1, 12, 0, 0);

            // configure the server once, in this static constructor
            //ConfigureServer(new TestServerOptions
            //{
            //    FrameworkVersion = "2.1.5",
            //    ServerUrl = "http://localhost:8888"
            //});

            DbHelper = new InMemoryDatabaseHelper(new TestServerOptions
            {
                FrameworkVersion = "2.1.5",
                ServerUrl = "http://localhost:8888"
            });
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

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            Db = ReusableDocumentStore.OpenAsyncSession();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            Db.Dispose();
        }
    }
}
