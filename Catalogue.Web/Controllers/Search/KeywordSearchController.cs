using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Http;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using Catalogue.Web.Search;
using System.Linq;

namespace Catalogue.Web.Controllers.Search
{
    public class KeywordSearchController : ApiController
    {
        private readonly ISearchService _keywordSearchService;

        public KeywordSearchController(ISearchService keywordSearchService)
        {
            _keywordSearchService = keywordSearchService;
        }

        public SearchOutputModel Get(string[] keywords, int n = 25, int p = 0)
        {
            var searchInputModel = new SearchInputModel()
            {
                Keywords = GetKeywords(keywords),
                NumberOfRecords = n,
                PageNumber = p,
                SearchType = SearchType.Keyword
            };
            var output = _keywordSearchService.FindByKeywords(searchInputModel);
            return output;
        }

        private List<MetadataKeyword> GetKeywords(string[] keywords)
        {
            var splitPattern = new Regex("");

            return (from k in keywords
                    where k.IsNotBlank()
                    select new MetadataKeyword()
                        {
                            Value = 
                        }).ToList();
        }
    }
}