using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Catalogue.Data.Model;
using Catalogue.Gemini.DataFormats;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using Raven.Client;

namespace Catalogue.Web.Controllers.Search
{
    public interface ISearchHelper
    {
        SearchOutputModel SearchByKeyword(QueryModel queryModel);
        SearchOutputModel SearchByText(QueryModel queryModel);
    }

    public class SearchHelper : ISearchHelper
    {
        private readonly IDocumentSession _db;

        public SearchHelper(IDocumentSession db)
        {
            _db = db;
        }

        public SearchOutputModel SearchByKeyword(QueryModel queryModel)
        {
            RavenQueryStatistics stats;

            var keyword = ParameterHelper.ParseKeywords(new [] {queryModel.K}).Single(); // for now, we only support one keyword

            var query = _db.Query<Record>()
                .Statistics(out stats)
                .Where(r => r.Gemini.Keywords.Any(k => k.Value == keyword.Value && k.Vocab == keyword.Vocab));

            int skipNumber = queryModel.P * queryModel.N;

            var results = query
                .Skip(skipNumber)
                .Take(queryModel.N)
                .ToList()
                .Select(r => new HalfBakedResult
                    {
                        Result = r,
                        Title = r.Gemini.Title.TruncateNicely(200),
                        Snippet = r.Gemini.Abstract.TruncateNicely(200)
                    });

            return MakeSearchOutputModel(queryModel, stats, results);
        }

        public SearchOutputModel SearchByText(QueryModel queryModel)
        {
            RavenQueryStatistics stats;
            FieldHighlightings titleLites;
            FieldHighlightings titleNLites;
            FieldHighlightings abstractLites;
            FieldHighlightings abstractNLites;

            var query = _db.Advanced.LuceneQuery<Record>("Records/Search")
                .Statistics(out stats)
                .Highlight("Title", 202, 1, out titleLites)
                .Highlight("TitleN", 202, 1, out titleNLites)
                .Highlight("Abstract", 202, 1, out abstractLites)
                .Highlight("AbstractN", 202, 1, out abstractNLites)
                .SetHighlighterTags("<b>", "</b>")
                .Search("Title", queryModel.Q).Boost(10)
                .Search("TitleN", queryModel.Q)
                .Search("Abstract", queryModel.Q)
                .Search("AbstractN", queryModel.Q);
            
            int skipNumber = queryModel.P * queryModel.N;
            
            var results = query
                    .Skip(skipNumber)
                    .Take(queryModel.N).ToList();

            var xs = from r in results
                     let titleFragments =
                         titleLites.GetFragments("records/" + r.Id).Concat(titleNLites.GetFragments("records/" + r.Id))
                     let abstractFragments =
                         abstractLites.GetFragments("records/" + r.Id)
                                      .Concat(abstractNLites.GetFragments("records/" + r.Id))
                     select new HalfBakedResult
                         {
                             Result = r,
                             Title = titleFragments.Select(f => f.TruncateNicely(200)).FirstOrDefault()
                                     ?? r.Gemini.Title.TruncateNicely(200),
                             Snippet = abstractFragments.Select(f => f.TruncateNicely(200)).FirstOrDefault()
                                       ?? r.Gemini.Abstract.TruncateNicely(200),
                         };

            return MakeSearchOutputModel(queryModel, stats, xs);
        }

        static SearchOutputModel MakeSearchOutputModel(
            QueryModel queryModel,
            RavenQueryStatistics stats,
            IEnumerable<HalfBakedResult> xs)
        {
            return new SearchOutputModel
                {
                    Total = stats.TotalResults,
                    Results = (from x in xs
                               let format = DataFormatQueries.GetDataFormatInfo(x.Result.Gemini.DataFormat)
                               select new ResultOutputModel
                                   {
                                       Id = x.Result.Id,
                                       Title = x.Title,
                                       // could be better. always want the whole title, highlighted
                                       Snippet = x.Snippet,
                                       Format = new FormatOutputModel
                                           {
                                               Group = format.Group,
                                               Glyph = format.Glyph,
                                               Name = format.Name,
                                           },
                                       Keywords = x.Result.Gemini.Keywords
                                                   .OrderBy(k => k.Vocab != "http://vocab.jncc.gov.uk/jncc-broad-category") // show first
                                                   .ThenBy(k => k.Vocab).ToList(),
                                       TopCopy = x.Result.TopCopy,
                                       Date = x.Result.Gemini.DatasetReferenceDate,
                                       ResourceType = x.Result.Gemini.ResourceType.FirstCharToUpper(),
                                   })
                        .ToList(),
                    Speed = stats.DurationMilliseconds,
                    Query =
                        new QueryOutputModel
                            {
                                K = queryModel.K,
                                Q = queryModel.Q,
                                P = queryModel.P,
                                N = queryModel.N,
                            }
                };
        }
    }

    class HalfBakedResult
    {
        public Record Result { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
    }
} 