using System.Web.Http;
using Catalogue.Data.Query;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        readonly IRecordQuerier querier;

        public SearchController(IRecordQuerier querier)
        {
            this.querier = querier;
        }

        // GET api/search?q=blah
        public RecordQueryOutputModel Get([FromUri] RecordQueryInputModel model)
        {
            return querier.SearchQuery(model);
        }
    }
}
