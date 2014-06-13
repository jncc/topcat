using System.Web.Http;
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
        public SearchOutputModel Get(string q, int n = 0, int p = 1)
        {
            return _searchService.Find(q, n, p);
        }
    }
}