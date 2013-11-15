using System;
using System.Linq;
using Catalogue.Data.Indexes;
using Catalogue.Tests.Utility;
using FluentAssertions;
using NUnit.Framework;
using Raven.Abstractions.Extensions;
using Raven.Client;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    class keywords_search_index_specs : DatabaseTestFixture
    {
        [Test]
        public void should_be_exactly_one_mesh_data_broad_category_result()
        {
            Db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
              .Where(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "Seabed-Habitat-Maps")
              .Count().Should().Be(1);
        }

        [Test]
        public void should_be_able_to_get_all_vocabs_in_order()
        {
            var results = Db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
                .OrderBy(r => r.Vocab)
                .Take(1000).ToList();

            results.Select(r => r.Value).ForEach(Console.WriteLine);

            results.GroupBy(r => r.Vocab).Select(g => g.Key).Should().ContainInOrder(new []
                {
                    "http://vocab.jncc.gov.uk/jncc-broad-category",
                    "http://vocab.jncc.gov.uk/mesh-gui",
                    "http://vocab.jncc.gov.uk/original-seabed-classification-system",
                    "http://vocab.jncc.gov.uk/reference-manager-code",
                    "http://vocab.jncc.gov.uk/seabed-map-status",
                    "http://vocab.jncc.gov.uk/seabed-survey-purpose",
                    "http://vocab.jncc.gov.uk/seabed-survey-technique",
                });
        }

        [Test]
        public void can_search_partial_matches_for_autocomplete()
        {
            // use the ngram search field for partial matches

            Db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
              .Search(r => r.ValueN, "se")
              .Count().Should().BeGreaterOrEqualTo(1);

            Db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
              .Search(r => r.ValueN, "seab")
              .Count().Should().Be(1);

            Db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
              .Search(r => r.ValueN, "seabe")
              .Count().Should().Be(1);
        }

        [Test]
        public void can_search_exact_matches()
        {
            // use the standard search field for exact matches

            Db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
              .Search(r => r.Value, "Seabed-Habitat-Maps")
              .Count().Should().Be(1);
        }

        [Test, Explicit]
        public void write_all_results()
        {
            var results = Db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
                .OrderBy(r => r.Vocab).ThenBy(r => r.Value)
                .Take(100)
                .ToList();

            foreach (var result in results)
            {
                Console.WriteLine(result.Vocab);
                Console.WriteLine(result.Value);
                Console.WriteLine();
            }
        }
    }
}
