using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        // GET api/search?q=blah
        public SearchOutputModel Get(string q, int p = 1)
        {
            var watch = Stopwatch.StartNew();

            using (var db = WebApiApplication.DocumentStore.OpenSession())
            {
                var results = db.Query<Records_Search.ReduceResult, Records_Search>() // Query<Record>("Records_Search")
                  .Search(x => x.Title, q)
                  .Take(10)
                  .ToList();

                return new SearchOutputModel
                    {
                        Total = results.Count,
                        Results = results.Select(r => new ResultOutputModel
                            {
                                Id = Guid.NewGuid(), // r.Id,
                                Title = r.Title,
                                Snippet = "", //r.Gemini.Abstract,
                            })
                            .ToList(),
                        Speed = watch.ElapsedMilliseconds,
                        Query = new QueryOutputModel { Q = q, P = p, }
                    };

            }

        }
    }
}