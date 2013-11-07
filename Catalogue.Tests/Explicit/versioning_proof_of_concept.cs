using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Tests.Utility;
using FluentAssertions;
using NUnit.Framework;
using Raven.Bundles.Versioning.Data;
using Raven.Client.Bundles.Versioning;
using Raven.Client.Embedded;

namespace Catalogue.Tests.Explicit
{
    class versioning_proof_of_concept : DatabaseTestFixture
    {
        [Test, Explicit]
        public void try_out_versioning()
        {
            using (var db = ReusableDocumentStore.OpenSession())
            {
                var record = db.Query<Record>().Customize(x => x.WaitForNonStaleResults())
                    .First(r => r.Gemini.Title == "St Tudwal's Islands (Tremadoc Bay) lifeforms map");

                record.Notes = "a revision";
                db.SaveChanges();
            }

            using (var db = ReusableDocumentStore.OpenSession())
            {
                var record = db.Query<Record>().Customize(x => x.WaitForNonStaleResults()).First();
                var revisions = db.Advanced.GetRevisionsFor<Record>("records/" + record.Id, 0, 10);

                revisions.Should().NotBeEmpty();
            }
        }

        
        [Test, Explicit]
        public void versioning_should_work()
        {
            var store = new EmbeddableDocumentStore { RunInMemory = true };

            store.Configuration.Settings.Add("Raven/ActiveBundles", "Versioning");
            store.Initialize();

            using (var db = store.OpenSession())
            {
                db.Store(new VersioningConfiguration
                {
                    Exclude = false,
                    Id = "Raven/Versioning/Items",
                    MaxRevisions = int.MaxValue

                });
                db.SaveChanges();
            }

            using (var db = store.OpenSession())
            {
                db.Store(new Item { Value = "first revision" });
                db.SaveChanges();
            }

            using (var db = store.OpenSession())
            {
                var item = db.Query<Item>().Customize(x => x.WaitForNonStaleResults()).First();
                item.Value = "second revision";
                db.SaveChanges();
            }

            using (var db = store.OpenSession())
            {
                var item = db.Query<Item>().Customize(x => x.WaitForNonStaleResults()).First();
                string id = db.Advanced.GetDocumentId(item);
                var revisions = db.Advanced.GetRevisionsFor<Item>(id, 0, 10);

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
