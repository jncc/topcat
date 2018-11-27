using System.Linq;
using Catalogue.Data;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents.Operations.Revisions;
using Raven.Client.Documents.Session;

namespace Catalogue.Tests.Explicit
{
    internal class versioning_proof_of_concept : DatabaseTestFixture
    {
        [Test, Explicit, Ignore("raven4")]
        public void versioning_should_work()
        {
            var store = DatabaseFactory.InMemory();

            //store.Configuration.Settings.Add("Raven/ActiveBundles", "Versioning");
            store.Initialize();

            using (IDocumentSession db = store.OpenSession())
            {
                //db.Store(new RevisionsConfiguration
                //{
                //    Exclude = false,
                //    Id = "Raven/Versioning/Items",
                //    MaxRevisions = int.MaxValue
                //});
                db.SaveChanges();
            }

            using (IDocumentSession db = store.OpenSession())
            {
                db.Store(new Item {Value = "first revision"});
                db.SaveChanges();
            }

            using (IDocumentSession db = store.OpenSession())
            {
                Item item = db.Query<Item>().Customize(x => x.WaitForNonStaleResults()).First();
                item.Value = "second revision";
                db.SaveChanges();
            }

            using (IDocumentSession db = store.OpenSession())
            {
                Item item = db.Query<Item>().Customize(x => x.WaitForNonStaleResults()).First();
                string id = db.Advanced.GetDocumentId(item);
                var revisions = db.Advanced.Revisions.GetFor<Item>(id, 0, 10);

                revisions.Should().NotBeEmpty();
            }
        }

        public class Item
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}