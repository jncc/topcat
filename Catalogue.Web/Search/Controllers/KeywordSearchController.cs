using System.Web.Http;
using Catalogue.Gemini.Model;
using Catalogue.Web.Search;
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

        public SearchOutputModel Post(SearchInputModel searchInputModel)
        {
            var output = _keywordSearchService.FindByKeyword(searchInputModel);
            return output;
        }
    }
}