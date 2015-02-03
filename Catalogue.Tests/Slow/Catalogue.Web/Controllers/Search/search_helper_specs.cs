using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using Catalogue.Web.Controllers.Search;
using Catalogue.Web.Search;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Search
{
    class search_helper_specs : DatabaseTestFixture
    {
        [Test]
        public void keyword_search_test()
        {
            var helper = new SearchHelper(Db);

            var input = new SearchInputModel
                {
                    Keywords = new [] {"vocab.jncc.gov.uk/jncc-broad-category/Seabed Habitat Maps"},
                    PageNumber = 0,
                    NumberOfRecords = 25
                };

            helper.KeywordSearch(input).Results.Count().Should().Be(25);
        }
    }
}
