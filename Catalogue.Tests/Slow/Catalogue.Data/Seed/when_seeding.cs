using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Tests.Slow.Catalogue.Data.Seed
{
    internal class when_seeding : DatabaseTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            var store = GetDocumentStore();
            store.Initialize();
            Seeder.Seed(store);
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            WaitForIndexing(store);
            ReusableDocumentStore = store;
            Db = ReusableDocumentStore.OpenSession();
        }

        [Test]
        public void should_seed_example_readonly_record()
        {
            var record = Db.Query<Record>()
                .First(r => r.Gemini.Title.StartsWith("An example read-only record"));

            record.ReadOnly.Should().BeTrue();
        }

        [Test]
        public void should_seed_small_box_record()
        {
            Db.Query<Record>().Count(r => r.Gemini.Title == "Small Box").Should().Be(1);
        }

        [Test]
        public void should_seed_vocabs()
        {
            Db.Query<Vocabulary>().Count().Should().Be(8);
        }
    }
}