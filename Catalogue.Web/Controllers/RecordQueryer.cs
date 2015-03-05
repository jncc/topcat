using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Gemini.DataFormats;
using Catalogue.Utilities.Text;
using Raven.Client;
using Raven.Client.Linq;

namespace Catalogue.Web.Controllers
{
    public interface IRecordQueryer
    {
        RecordQueryOutputModel SearchQuery(RecordQueryInputModel input);
        IEnumerable<Record> RecordQuery(RecordQueryInputModel input);
    }

    public class RecordQueryer : IRecordQueryer
    {
        private readonly IDocumentSession _db;

        public RecordQueryer(IDocumentSession db)
        {
            _db = db;
        }

        RavenQueryStatistics stats;
        FieldHighlightings titleLites;
        FieldHighlightings titleNLites;
        FieldHighlightings abstractLites;
        FieldHighlightings abstractNLites;

        public IEnumerable<Record> RecordQuery(RecordQueryInputModel input)
        {
            var query = _db.Query<RecordIndex.Result, RecordIndex>()
                .Statistics(out stats)
                .Customize(x => x.Highlight("Title", 202, 1, out titleLites))
                .Customize(x => x.Highlight("TitleN", 202, 1, out titleNLites))
                .Customize(x => x.Highlight("Abstract", 202, 1, out abstractLites))
                .Customize(x => x.Highlight("AbstractN", 202, 1, out abstractNLites))
                .Customize(x => x.SetHighlighterTags("<b>", "</b>"));

            if (input.Q.IsNotBlank())
            {
                query = query
                    .Search(r => r.Title, input.Q, boost: 10)
                    .Search(r => r.TitleN, input.Q)
                    .Search(r => r.Abstract, input.Q)
                    .Search(r => r.AbstractN, input.Q);
                    // .Search(r => r.Keywords, input.Q)
            }

            if (input.HasKeywords())
            {
                foreach (var keyword in ParameterHelper.ParseKeywords(input.K))
                {
                    string k = keyword.Vocab + "/" + keyword.Value;
                    query = query.Where(r => r.Keywords.Contains(k));
                }
            }

            return query.As<Record>() // project the query from the index result type to the actual document type
                    .Skip(input.P * input.N)
                    .Take(input.N)
                    .ToList();
        }

        public RecordQueryOutputModel SearchQuery(RecordQueryInputModel input)
        {
            var xs = from r in RecordQuery(input)
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

            return MakeOutput(input, stats, xs);
        }


        static RecordQueryOutputModel MakeOutput(
            RecordQueryInputModel input,
            RavenQueryStatistics stats,
            IEnumerable<HalfBakedResult> xs)
        {
            return new RecordQueryOutputModel
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
                    Query = input
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
