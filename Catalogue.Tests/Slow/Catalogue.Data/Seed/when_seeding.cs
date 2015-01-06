using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Seed
{
    internal class when_seeding : DatabaseTestFixture
    {
        // the DatabaseTestFixture will already have run the seeder via the import
        // so cheat and do some sanity tests

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
            var vocabs = Db.Query<Vocabulary>().ToList();
            vocabs.Count.Should().BeGreaterThan(3); // there are several vocabs in the mesh data
        }
    }
}