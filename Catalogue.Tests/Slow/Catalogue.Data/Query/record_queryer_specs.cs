using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catalogue.Data.Query;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Query
{
    class record_queryer_specs : DatabaseTestFixture
    {

        RecordQueryInputModel EmptyQuery()
        {
                return new RecordQueryInputModel
                {
                    Q = "",
                    K = new string[0],
                    P = 0,
                    N = 25,
                };

        }

        object[] KeywordTestCases =
        {
            new object[] { "", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction ", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction (abstraction)", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction – Water (abstraction)", 1 }, // note unicode dash!!
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction – Water", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction", 0 },
            new object[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps", 189 },
            new object[] { "vocab.jncc.gov.uk/some-vocab/Two words", 1 },
        };

        [Test, TestCaseSource("KeywordTestCases")]
        public void should_return_exact_matches_for_keywords(string keyword, int expected)
        {
            var input = EmptyQuery().With(q => q.K = new [] { keyword });
            var output = new RecordQueryer(Db).SearchQuery(input);
            
            output.Total.Should().Be(expected);
        }
    }
}
