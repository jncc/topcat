using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents.Session;
using System.Linq;

namespace Catalogue.Tests.Slow
{
    public class when_testing_this_application : DatabaseTestFixture
    {
        [Test]
        public void should_be_able_to_query_test_data()
        {
            using (IDocumentSession db = ReusableDocumentStore.OpenSession())
            {
                db.Query<Record>().Count().Should().BePositive();
            }
        }

    }
}