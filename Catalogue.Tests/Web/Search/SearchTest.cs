using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using Catalogue.Web.Search;
using Catalogue.Web.Search.Service;
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
        private SearchRepository _searchRepository;
        private SearchService _searchService;
        private const int PageSize = 25;

        [TestFixtureSetUp]
        public void setUp()
        {
            _searchRepository = new SearchRepository(Db);
            _searchService = new SearchService(_searchRepository);
        }
        [Test]
        public void WhenPagingCheckCountIsAsExpected()
        {
            // do not perform a full text search, so should be fewer results
            var results = _searchService.Find(_searchInputModel);
            Assert.AreEqual(results.Results.Count, 25);
            var TotalReturned = results.Results.Count;
            // loop through each page
            int pages = (results.Total + PageSize - 1) / PageSize;
            for (int i = 1; i <= pages; i++)
            {
                _searchInputModel.PageNumber = i;
                results = _searchService.Find(_searchInputModel);
                TotalReturned += results.Results.Count;
            }
            Assert.AreEqual(results.Total,TotalReturned);
        }
    }
}
