using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        // GET api/search?q=blah
        public SearchOutputModel Get(string q)
        {
            var data = new List<ResultOutputModel>
                        {
                            new ResultOutputModel { Id = Guid.NewGuid(), Title = "Some result", Snippet = "This is some result" },
                            new ResultOutputModel { Id = Guid.NewGuid(), Title = "Another result", Snippet = "This is another result" },
                            new ResultOutputModel { Id = Guid.NewGuid(), Title = "Yet another result", Snippet = "This is yet another result" },
                        };

            var watch = Stopwatch.StartNew();
            var results = data.Where(d => d.Title.Contains(q) || d.Snippet.Contains(q)).ToList();

            Thread.Sleep(1000);
            return new SearchOutputModel
                {
                    Total = results.Count,
                    Results = results,
                    Speed = watch.ElapsedMilliseconds,
                    Query = q,
                };
        }
    }
}