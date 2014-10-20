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

        public SearchOutputModel Get(string value, string vocab, int n = 25, int p = 0)
        {
            var keyword = new MetadataKeyword()
            {
                Value = value,
                Vocab = vocab
            };
            var searchInputModel = new SearchInputModel()
            {
                Keyword = keyword,
                NumberOfRecords = n,
                PageNumber = p,
                SearchType = SearchType.Keyword
            };
            var output = _keywordSearchService.FindByKeyword(searchInputModel);
            return output;
        }
    }
}