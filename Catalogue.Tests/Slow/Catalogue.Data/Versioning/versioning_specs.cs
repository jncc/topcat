using System;
using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Gemini.Templates;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Catalogue.Tests.Slow.Catalogue.Data.Versioning
{
    internal class versioning_specs
    {
        [Test, Ignore("raven4")]
        public void versioning_should_work()
        {
            // guid keys are problematic for raven versioning
            // so here's a test that show that it now works

            // this test mutates data so we don't use the ResusableDocumentStore...

            IDocumentStore store = new InMemoryDatabaseHelper().Create();
            var id = ("records/f7b444f7-76f3-47a4-b8d8-cc204d400728");

            using (IDocumentSession db = store.OpenSession())
            {
                var record = new Record {Id = id, Gemini = Library.Example()};

                db.Store(record);
                db.SaveChanges();
            }

            using (IDocumentSession db = store.OpenSession())
            {
                var record = db.Load<Record>(id);

                record.Notes = "i'm updating this record!";
                db.SaveChanges();
            }

            using (IDocumentSession db = store.OpenSession())
            {
                var record = db.Load<Record>(id);
                var revisions = db.Advanced.Revisions.GetFor<Record>(db.Advanced.GetDocumentId(record), 0, 10);

                revisions.Count().Should().Be(2);
                revisions.Select(r => r.Revision).Should().ContainInOrder(new[] {1, 2});
            }
        }
    }
}