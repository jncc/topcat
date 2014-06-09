using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Seed
{
    class when_seeding : DatabaseTestFixture
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
    }
}
