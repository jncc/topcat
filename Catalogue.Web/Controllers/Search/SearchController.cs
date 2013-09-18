using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        // GET api/search?q=blah
        public SearchOutputModel Get(string q)
        {
            return new SearchOutputModel
                {
                    TotalResults = 3,
                    Results = new List<ResultOutputModel>
                        {
                            new ResultOutputModel { Id = Guid.NewGuid(), Title = "Some result", Snippet = "This is some result" },
                            new ResultOutputModel { Id = Guid.NewGuid(), Title = "Another result", Snippet = "This is another result" },
                            new ResultOutputModel { Id = Guid.NewGuid(), Title = "Yet another result", Snippet = "This is yet another result" },
                        }
                };
        }
    }
}