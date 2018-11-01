using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Test;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Documents.Session;
using System;
using System.Linq;
using Catalogue.Data.Seed;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Tests.Slow.Catalogue.Data.Query
{
    public class record_querier_filter_specs_using_seeder : SeededDbTest
    {
        static object[] KeywordTestCases =
        {
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction ", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction (abstraction)", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction – Water (abstraction)", 1 }, // note unicode dash!!
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction – Water", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction", 0 },
            new object[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps", 189 },
            new object[] { "vocab.jncc.gov.uk/some-vocab/Two words", 1 },
        };

        [Test, TestCaseSource(nameof(KeywordTestCases))]
        public void should_return_correct_result_count_for_keywords(string keyword, int expected)
        {
            var input = QueryTestHelper.EmptySearchInput().With(q => q.F = new FilterOptions { Keywords = new[] { keyword } });
            var output = new RecordQueryer(Db).Search(input);

            output.Total.Should().Be(expected);
        }

        [Test]
        public void keyword_search_test()
        {
            var helper = new RecordQueryer(Db);

            var input = new RecordQueryInputModel
            {
                Q = "",
                F = new FilterOptions { Keywords = new[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" } },
                P = 0,
                N = 25,
                O = 0
            };

            helper.Search(input).Results.Count().Should().Be(25);
        }
    }
}
