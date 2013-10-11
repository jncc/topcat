using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Raven.Abstractions.Data;
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
                FieldHighlightings titleLites;
                FieldHighlightings abstractLites;

                string pattern = q; // +"*";

                var query = db.Advanced.LuceneQuery<Record>("Records/Search")
                                .Statistics(out stats)
                                .Highlight("Title", 500, 1, out titleLites)
                                .Highlight("Abstract", 128, 2, out abstractLites)
                                .SetHighlighterTags("<strong>", "</strong>")
                                .Search("Title", pattern).Boost(10)
                                .Search("Abstract", pattern);

                var results = query.Take(25).ToList();

                //var suggestions = db.Query<Record>() query..Suggest(new SuggestionQuery { Field = "Name", Term = "Orin" });

                var xs = from r in results
                            select new
                                {
                                    result = r,
                                    titleFragments = titleLites.GetFragments("records/" + r.Id),
                                    abstractFragments = abstractLites.GetFragments("records/" + r.Id),
                                };

//                var fs = lites.GetFragments("Records/" + results.First().Id);
                

                return new SearchOutputModel
                    {
                        Total = stats.TotalResults,
                        Results = xs.Select(x => new ResultOutputModel
                            {
                                Id = x.result.Id,
                                Title = x.titleFragments.FirstOrDefault() ?? x.result.Gemini.Title, // x.result.Gemini.Title,
                                Snippet = x.abstractFragments.FirstOrDefault() ?? x.result.Gemini.Abstract, //.Gemini.Abstract,
                            })
                            .ToList(),
                        Speed = watch.ElapsedMilliseconds,
                        Query = new QueryOutputModel { Q = q, P = p, }
                    };

            }

        }
    }
}