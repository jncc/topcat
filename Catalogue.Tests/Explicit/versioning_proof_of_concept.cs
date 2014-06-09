using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raven.Bundles.Versioning.Data;
using Raven.Client;
using Raven.Client.Bundles.Versioning;
using Raven.Client.Embedded;

namespace Catalogue.Tests.Explicit
{
    internal class versioning_proof_of_concept : DatabaseTestFixture
    {
        [Test, Explicit]
        public void versioning_should_work()
        {
            var store = new EmbeddableDocumentStore {RunInMemory = true};

            store.Configuration.Settings.Add("Raven/ActiveBundles", "Versioning");
            store.Initialize();

            using (IDocumentSession db = store.OpenSession())
            {
                db.Store(new VersioningConfiguration
                {
                    Exclude = false,
                    Id = "Raven/Versioning/Items",
                    MaxRevisions = int.MaxValue
                });
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
                Item[] revisions = db.Advanced.GetRevisionsFor<Item>(id, 0, 10);

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