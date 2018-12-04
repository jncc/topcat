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
    public class CleanDbTest : DatabaseTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            ReusableDocumentStore = DbHelper.Create();
        }

        [TearDown]
        public void TearDown()
        {
            ReusableDocumentStore.Dispose();
            Dispose();
        }
    }
}
