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
    public class SearchKeywordController : ApiController
    {
        private readonly IKeywordSearchService _keywordSearchService;

        public SearchKeywordController(IKeywordSearchService keywordSearchService)
        {
            _keywordSearchService = keywordSearchService;
        }

        public SearchOutputModel Get(string keyword)
        {
            return _keywordSearchService.find(keyword);
        }
    }
}
