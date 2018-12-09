using Catalogue.Data.Seed;
using NUnit.Framework;
using Raven.Client.Documents.Session;

namespace Catalogue.Tests
{
    /// <summary>
    /// Extend this class for tests that run against seeded records
    /// use the Db references
    /// </summary>
    public class SeededDbTest : DatabaseTestFixture
    {
        /// <summary>
        /// A document session open for the lifetime of the test fixture.
        /// </summary>
        protected IDocumentSession Db;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ReusableDocumentStore = DbHelper.Create(Seeder.Seed);
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
