using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Gemini.DataFormats;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using Catalogue.Web.Controllers.Search;
using Raven.Client;

namespace Catalogue.Web.Search
{
    public interface ISearchRepository
    {
        SearchOutputModel FindByKeyword(string searchTerm, int n, int page);
        SearchOutputModel Find(string searchTerm, int n, int page);
    }

    public class SearchRepository : ISearchRepository
    {
        private readonly IDocumentSession _db;


        public SearchRepository(IDocumentSession db)
        {
            this._db = db;
        }

        public SearchOutputModel FindByKeyword(string searchTerm, int n = 0, int page = 1)
        {
            if (n == 0)
            {
                
            }
            RavenQueryStatistics stats;
            FieldHighlightings titleLites;
            FieldHighlightings titleNLites;
            FieldHighlightings abstractLites;
            FieldHighlightings abstractNLites;
            KeywordsSearchIndex.Result keywordResults = _db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>().FirstOrDefault(r => r.Value.Equals("seabed habitat maps"));
            
            IQueryable<Record> query =  _db.Query<Record>()
                .Statistics(out stats)
                .Where(r => r.Gemini.Keywords.Contains(new Keyword(keywordResults.Value, keywordResults.Vocab)));

            List<Record> results;
            if (n > 0)
            {
                results = query.Take(n).ToList();

            }
            else
            {
                results = query.ToList();
            }




            return new SearchOutputModel
            {
                Total = stats.TotalResults,
                Results = (from x in results
                           let format = DataFormatQueries.GetDataFormatInfo(x.Gemini.DataFormat)
                           select new ResultOutputModel
                           {
                               Id = x.Id,
                               Title = x.Gemini.Title.TruncateNicely(200),
                               // could be better. always want the whole title, highlighted
                               Snippet = x.Gemini.Abstract.TruncateNicely(200),
                               Format = new FormatOutputModel
                               {
                                   Group = format.Group,
                                   Glyph = format.Glyph,
                                   Name = format.Name,
                               },
                               Keywords = x.Gemini.Keywords
                                   .OrderBy(k => k.Vocab != "http://vocab.jncc.gov.uk/jncc-broad-category") // show first
                                   .ThenBy(k => k.Vocab).ToList(),
                               TopCopy = x.TopCopy,
                               Date = x.Gemini.DatasetReferenceDate.Truncate(4),
                           })
                    .ToList(),
                Speed = stats.DurationMilliseconds,
                Query = new QueryOutputModel { Q = searchTerm, P = page, }
            };
           
        }

        public SearchOutputModel Find(string searchTerm, int n=0, int page = 1 )
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
                .Search("Title", searchTerm).Boost(10)
                .Search("TitleN", searchTerm)
                .Search("Abstract", searchTerm)
                .Search("AbstractN", searchTerm);

            List<Record> results;

            if (n > 0)
            {
                results = query.Take(25).ToList();
            }
            else
            {
                results = query.ToList();
            }

            var xs = from r in results
                     select new
                     {
                         result = r,
                         titleFragments =
                             titleLites.GetFragments("records/" + r.Id).Concat(titleNLites.GetFragments("records/" + r.Id)),
                         abstractFragments =
                             abstractLites.GetFragments("records/" + r.Id)
                                 .Concat(abstractNLites.GetFragments("records/" + r.Id)),
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
                                       ?? x.result.Gemini.Title.TruncateNicely(200),
                               // could be better. always want the whole title, highlighted
                               Snippet = x.abstractFragments.Select(f => f.TruncateNicely(200)).FirstOrDefault()
                                         ?? x.result.Gemini.Abstract.TruncateNicely(200),
                               Format = new FormatOutputModel
                               {
                                   Group = format.Group,
                                   Glyph = format.Glyph,
                                   Name = format.Name,
                               },
                               Keywords = x.result.Gemini.Keywords
                                   .OrderBy(k => k.Vocab != "http://vocab.jncc.gov.uk/jncc-broad-category") // show first
                                   .ThenBy(k => k.Vocab).ToList(),
                               TopCopy = x.result.TopCopy,
                               Date = x.result.Gemini.DatasetReferenceDate.Truncate(4),
                           })
                    .ToList(),
                Speed = stats.DurationMilliseconds,
                Query = new QueryOutputModel { Q = searchTerm, P = page, }
            };
            
        }
    }
}