using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Catalogue.Gemini.Spatial;
using Catalogue.Web.Search.Service;
using Raven.Client;

namespace Catalogue.Web.Controllers.Search
{
    public class KeywordSearchController : ApiController
    {
        private readonly ISearchService _keywordSearchService;

        public KeywordSearchController(ISearchService keywordSearchService)
        {
            _keywordSearchService = keywordSearchService;
        }

        public SearchOutputModel Get(string keyword, int n = 0, int page = 1)
        {
            return _keywordSearchService.FindByKeyword(keyword, n, page);
        }
    }
}
