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

        private List<MetadataKeyword> GetKeywords(IEnumerable<string> keywords)
        {
            return (from k in keywords
                    where k.IsNotBlank()
                    from m in Regex.Matches(k, @"^([\w/]*)/(\w*)$").Cast<Match>()
                    let pair = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1)
                    select new MetadataKeyword
                        {
                            Vocab = "http://" + pair.ElementAt(0).Trim(),
                            Value = pair.ElementAt(1).Trim()
                        }
                   ).ToList();

        }
    }
}