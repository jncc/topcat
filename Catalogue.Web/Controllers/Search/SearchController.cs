using System.Web.Http;
using Catalogue.Data.Query;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        readonly IRecordQueryer _queryer;

        public SearchController(IRecordQueryer _queryer)
        {
            this._queryer = _queryer;
        }

        // GET api/search?q=blah
        public RecordQueryOutputModel Get([FromUri] RecordQueryInputModel model)
        {
            return _queryer.SearchQuery(model);
        }
    }
}
