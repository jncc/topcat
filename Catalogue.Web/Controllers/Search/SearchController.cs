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
                RavenQueryStatistics stats;
                FieldHighlightings lites;

                var results = db.Advanced.LuceneQuery<Record>("Records/Search")
                                .Statistics(out stats)
                                .Highlight("Title", 128, 2, out lites)
                                .SetHighlighterTags("<strong>", "</strong>")
                                .Search("Title", q).Boost(10)
                                .Search("Abstract", q)
                                .Take(25)
                                .ToList();

                
//                var fs = lites.GetFragments("Records/" + results.First().Id);
                

                return new SearchOutputModel
                    {
                        Total = stats.TotalResults,
                        Results = results.Select(r => new ResultOutputModel
                            {
                                Id = r.Id,
                                Title = r.Gemini.Title,
                                Snippet = r.Gemini.Abstract,
                            })
                            .ToList(),
                        Speed = watch.ElapsedMilliseconds,
                        Query = new QueryOutputModel { Q = q, P = p, }
                    };

            }

        }
    }
}