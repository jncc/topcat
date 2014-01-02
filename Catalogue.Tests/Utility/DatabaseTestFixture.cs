using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Tests.Utility
{
    public class DatabaseTestFixture
    {
        public static IDocumentStore ReusableDocumentStore { get; set; }

        static DatabaseTestFixture()
        {
            // initialise the ResusableDocumentStore once, in this static constructor

            var store = DatabaseHelper.CreateInMemoryStore(Seeder.Seed); // seed with test data
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
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Db.Dispose();
        }
    }
}
