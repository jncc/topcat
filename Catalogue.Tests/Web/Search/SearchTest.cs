using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using Catalogue.Web.Controllers;
using Catalogue.Web.Controllers.Search;
using NUnit.Framework;

namespace Catalogue.Tests.Web.Search
{
    internal class SearchTest : DatabaseTestFixture
    {
        private readonly QueryModel _queryModel = new QueryModel()
        {
            Q = "se", 
            N= 25,
            P= 0
        };

        private SearchHelper searchHelper;
        private const int PageSize = 25;

        [TestFixtureSetUp]
        public void setUp()
        {
            searchHelper = new SearchHelper(Db);
        }
        [Test]
        public void WhenPagingCheckCountIsAsExpected()
        {
            // do not perform a full text search, so should be fewer results
            var results = searchHelper.Search(_queryModel);
            Assert.AreEqual(results.Results.Count, 25);
            var totalReturned = results.Results.Count;
            // loop through each page
            int pages = (results.Total + PageSize - 1) / PageSize;
            for (int i = 1; i <= pages; i++)
            {
                _queryModel.P = i;
                results = searchHelper.Search(_queryModel);
                totalReturned += results.Results.Count;
            }
            Assert.AreEqual(results.Total,totalReturned);
        }

    
    }
}
