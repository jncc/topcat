using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using Catalogue.Web.Controllers.Search;
using Catalogue.Web.Search;
using NUnit.Framework;

namespace Catalogue.Tests.Web.Search
{
    internal class SearchTest : DatabaseTestFixture
    {
        private readonly SearchInputModel _searchInputModel = new SearchInputModel()
        {
            Query = "se", 
            NumberOfRecords= 25,
            PageNumber= 0
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
            var results = searchHelper.FullTextSearch(_searchInputModel);
            Assert.AreEqual(results.Results.Count, 25);
            var totalReturned = results.Results.Count;
            // loop through each page
            int pages = (results.Total + PageSize - 1) / PageSize;
            for (int i = 1; i <= pages; i++)
            {
                _searchInputModel.PageNumber = i;
                results = searchHelper.FullTextSearch(_searchInputModel);
                totalReturned += results.Results.Count;
            }
            Assert.AreEqual(results.Total,totalReturned);
        }

    
    }
}
