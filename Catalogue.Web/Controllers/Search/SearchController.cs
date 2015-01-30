using System.Web.Http;
using Catalogue.Web.Search;

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
        public SearchOutputModel Get(string q, int n = 25, int p = 0)
        {
            SearchInputModel searchInputModel = new SearchInputModel()
            {
                PageNumber = p,
                Query = q,
                NumberOfRecords = n
            };
            var output = _searchHelper.FullTextSearch(searchInputModel);
            return output;
        }
    }
}