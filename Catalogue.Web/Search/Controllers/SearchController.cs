﻿using System.Web.Http;
using Catalogue.Web.Search;
using Catalogue.Web.Search.Service;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        // GET api/search?q=blah
        public SearchOutputModel Get(string q, int n = 25, int p = 0)
        {
            SearchInputModel searchInputModel = new SearchInputModel()
            {
                PageNumber = p,
                Query = q,
                NumberOfRecords = n
            };
            var output = _searchService.Find(searchInputModel);
            return output;
        }
    }
}