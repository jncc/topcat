using System;
using System.IO;
using System.Linq;
using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;

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