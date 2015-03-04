using System.Web.Http;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        readonly ISearchHelper helper;

        public SearchController(ISearchHelper helper)
        {
            this.helper = helper;
        }

        // GET api/search?q=blah
        public SearchOutputModel Get([FromUri] QueryModel model)
        {
            return helper.Search(model);
        }
    }
}
