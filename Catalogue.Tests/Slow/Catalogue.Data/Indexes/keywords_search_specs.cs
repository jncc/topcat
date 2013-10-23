using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using Catalogue.Tests.Utility;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    class keywords_search_specs : DatabaseTestFixture
    {
        [Test]
        public void should_be_able_to_query_keywords()
        {
            var results = Db.Query<Keyword>("Keywords/Search")
              .Search(k => k.Value, "SeabedHabitatMaps")
              .Take(10)
              .ToList();

            results.Count.Should().Be(10);
        }
    }
}
