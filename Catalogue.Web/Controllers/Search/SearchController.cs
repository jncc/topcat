using System.Linq;
using System.Web.Http;
using Catalogue.Data.Model;
using Catalogue.Gemini.DataFormats;
using Catalogue.Utilities.Text;
using Raven.Client;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        readonly IDocumentSession db;

        public SearchController(IDocumentSession db)
        {
            this.db = db;
        }

        // GET api/search?q=blah
        public SearchOutputModel Get(string q, int p = 1)
        {
            RavenQueryStatistics stats;
            FieldHighlightings titleLites;
            FieldHighlightings titleNLites;
            FieldHighlightings abstractLites;
            FieldHighlightings abstractNLites;

            var query = db.Advanced.LuceneQuery<Record>("Records/Search")
                            .Statistics(out stats)
                            .Highlight("Title", 202, 1, out titleLites)
                            .Highlight("TitleN", 202, 1, out titleNLites)
                            .Highlight("Abstract", 202, 1, out abstractLites)
                            .Highlight("AbstractN", 202, 1, out abstractNLites)
                            .SetHighlighterTags("<b>", "</b>")
                            .Search("Title", q).Boost(10)
                            .Search("TitleN", q)
                            .Search("Abstract", q)
                            .Search("AbstractN", q);

            var results = query.Take(25).ToList();

            var xs = from r in results
                        select new
                        {
                            result = r,
                            titleFragments = titleLites.GetFragments("records/" + r.Id).Concat(titleNLites.GetFragments("records/" + r.Id)),
                            abstractFragments = abstractLites.GetFragments("records/" + r.Id).Concat(abstractNLites.GetFragments("records/" + r.Id)),
                        };                

            return new SearchOutputModel
                {
                    Total = stats.TotalResults,
                    Results = (from x in xs
                                let format = DataFormatQueries.GetDataFormatInfo(x.result.Gemini.DataFormat)
                                select new ResultOutputModel
                                    {
                                        Id = x.result.Id,
                                        Title = x.titleFragments.Select(f => f.TruncateNicely(200)).FirstOrDefault()
                                            ?? x.result.Gemini.Title.TruncateNicely(200), // could be better. always want the whole title, highlighted
                                        Snippet = x.abstractFragments.Select(f => f.TruncateNicely(200)).FirstOrDefault()
                                            ?? x.result.Gemini.Abstract.TruncateNicely(200),
                                        Format = new FormatOutputModel
                                            {
                                                Group = format.Group,
                                                Glyph = format.Glyph,
                                                Name = format.Name,
                                            },
                                        Keywords = x.result.Gemini.Keywords.OrderBy(k => k.Vocab).ToList(),
                                        TopCopy = x.result.TopCopy,
                                    })
                                .ToList(),
                    Speed = stats.DurationMilliseconds,
                    Query = new QueryOutputModel { Q = q, P = p, }
                };
        }
    }
}
