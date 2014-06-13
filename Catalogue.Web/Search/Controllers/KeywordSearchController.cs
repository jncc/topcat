using System.Web.Http;
using Catalogue.Gemini.Model;
using Catalogue.Web.Search.Service;

namespace Catalogue.Web.Controllers.Search
{
    public class KeywordSearchController : ApiController
    {
        private readonly ISearchService _keywordSearchService;

        public KeywordSearchController(ISearchService keywordSearchService)
        {
            _keywordSearchService = keywordSearchService;
        }

        public SearchOutputModel Post(Keyword keyword, int n = 0, int page = 1)
        {
            return _keywordSearchService.FindByKeyword(keyword, n, page);
        }
    }
}