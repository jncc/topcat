using Catalogue.Data;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
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
            // initialise the ResusableDocumentStore once, in this static constructor
            ReusableDocumentStore = DatabaseFactory.Create(DatabaseFactory.DatabaseConnectionType.ReUseable); ;
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
}
