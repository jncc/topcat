using System.Linq;
using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow
{
    public class when_testing_this_application : DatabaseTestFixture
    {
        [Test]
        public void should_be_able_to_query_test_data()
        {
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Query<Item>().Count().Should().BePositive();
            }
        }
    }
}
