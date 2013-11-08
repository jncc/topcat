using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Tests.Utility;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Seed
{
    class when_seeding : DatabaseTestFixture
    {
        [Test]
        public void should_seed_example_readonly_record()
        {
            // the DatabaseTestFixture will already have run the seeder via the import
            // so do a "bad" sanity test
            var record = Db.Query<Record>()
                .First(r => r.Gemini.Title.StartsWith("An example read-only record"));

            record.ReadOnly.Should().BeTrue();
        }
    }
}
