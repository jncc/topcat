using System.Linq;
using Catalogue.Web.Controllers;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Search
{
    class search_helper_specs : DatabaseTestFixture
    {
        [Test]
        public void keyword_search_test()
        {
            var helper = new RecordQueryer(Db);

            var input = new RecordQueryInputModel
                {
                    Q = "",
                    K = new [] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" },
                    P = 0,
                    N = 25
                };

            helper.SearchQuery(input).Results.Count().Should().Be(25);
        }
    }
}
