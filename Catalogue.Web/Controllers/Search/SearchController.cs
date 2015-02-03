using System.Web.Http;
using Catalogue.Web.Search;
using System.Linq;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        private readonly ISearchHelper _searchHelper;

        public SearchController(ISearchHelper searchHelper)
        {
            _searchHelper = searchHelper;
        }

        // GET api/search?q=blah
        public SearchOutputModel Get(string q, int n = 25, int p = 0, SearchType t = SearchType.FullText)
        {
            SearchInputModel searchInputModel = new SearchInputModel()
            {
                PageNumber = p,
                Query = q, //should only be one full text search term but q could be an arrary for keywords
                NumberOfRecords = n,
                SearchType = t
            };
            var output = _searchHelper.FullTextSearch(searchInputModel);
            return output;
        }
    }
}