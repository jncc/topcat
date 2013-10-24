using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Indexes;
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
            Db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
              .Where(k => k.Value == "SeabedHabitatMaps")
              .Count().Should().Be(1);

            var results = Db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
              .Take(100)
              .ToList();

            foreach (var result in results)
            {
                Console.WriteLine(result.Key);
                Console.WriteLine(result.Vocab);
                Console.WriteLine(result.Value);
                Console.WriteLine();
            }

        }
    }
}
