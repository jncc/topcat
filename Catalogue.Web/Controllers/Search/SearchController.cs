using System;
using System.Linq;
using System.Web.Http;
using Catalogue.Utilities.Text;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        readonly ISearchHelper searchHelper;

        public SearchController(ISearchHelper searchHelper)
        {
            this.searchHelper = searchHelper;
        }

        // GET api/search?q=blah
        public SearchOutputModel Get([FromUri]QueryModel model)
        {
            if (model.Q.IsNotBlank() && model.HasKeywords)
            {
                throw new NotSupportedException("Full text plus keyword queries aren't currently supported.");
            }
            else if (model.HasKeywords)
            {
                return searchHelper.SearchByKeyword(model);
            }
            else
            {
                return searchHelper.Search(model);
            }
        }
    }
}
