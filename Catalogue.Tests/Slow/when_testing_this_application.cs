using System;
using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Gemini.Templates;
using Catalogue.Tests.Utility;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Bundles.Versioning;

namespace Catalogue.Tests.Slow
{
    public class when_testing_this_application : DatabaseTestFixture
    {
        [Test]
        public void should_be_able_to_query_test_data()
        {
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Query<Record>().Count().Should().BePositive();
            }
        }

        [Test]
        public void versioning_should_work()
        {
            // guid keys are problematic for raven versioning
            // so here's a test that show that it now works

            // this test mutates data so we don't use the ResusableDocumentStore...

            var store = new InMemoryDatabaseHelper().Create();
            var id = Guid.Parse("f7b444f7-76f3-47a4-b8d8-cc204d400728");

            using (var db = store.OpenSession())
            {
                var record = new Record { Id = id, Gemini = Library.Example() };

                db.Store(record);
                db.SaveChanges();
            }

            using (var db = store.OpenSession())
            {
                var record = db.Load<Record>(id);

                record.Notes = "i'm updating this record!";
                db.SaveChanges();
            }

            using (var db = store.OpenSession())
            {
                var record = db.Load<Record>(id);
                var revisions = db.Advanced.GetRevisionsFor<Record>(db.Advanced.GetDocumentId(record), 0, 10);

                revisions.Count().Should().Be(2);
                revisions.Select(r => r.Revision).Should().ContainInOrder(new [] { 1, 2 });
            }
        }
    }
}
