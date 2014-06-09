using System.Linq;
using System.Web.Http;
using Catalogue.Data.Model;
using Catalogue.Gemini.DataFormats;
using Catalogue.Utilities.Text;
using Catalogue.Web.Search.Service;
using Raven.Client;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        
        readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            
            _searchService = searchService;
        }

        // GET api/search?q=blah
        public SearchOutputModel Get(string q, int p = 1)
        {
            return _searchService.Find(q, p);
        }
    }
}
