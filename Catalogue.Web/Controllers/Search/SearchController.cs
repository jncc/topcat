using System.Web.Http;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        readonly IRecordQueryer queryer;

        public SearchController(IRecordQueryer queryer)
        {
            this.queryer = queryer;
        }

        // GET api/search?q=blah
        public RecordQueryOutputModel Get([FromUri] RecordQueryInputModel model)
        {
            return queryer.SearchQuery(model);
        }
    }
}
