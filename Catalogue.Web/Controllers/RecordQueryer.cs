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

        // these fields are member variables only so they can be accessed from both query methods
        RavenQueryStatistics stats;
        FieldHighlightings titleLites;
        FieldHighlightings titleNLites;
        FieldHighlightings abstractLites;
        FieldHighlightings abstractNLites;

        /// <summary>
        /// A general-purpose query that returns records.
        /// Can be materialised as-is, or customised further (see SearchQuery method).
        /// We may need to refactor this to support ravendb streaming for larger result sets.
        /// </summary>
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
                    .Search(r => r.AbstractN, input.Q)
                    .Search(r => r.KeywordsN, input.Q);
            }

            if (input.HasKeywords())
            {
                foreach (var keyword in ParameterHelper.ParseKeywords(input.K))
                {
                    string k = keyword.Vocab + "/" + keyword.Value;
                    query = query.Where(r => r.Keywords.Contains(k));
                }
            }

            return query.As<Record>() // ravendb method to project from the index result type to the actual document type
                    .Skip(input.P * input.N)
                    .Take(input.N)
                    .ToList();
        }

        /// <summary>
        /// A query for the Google-style search results page.
        /// </summary>
        public RecordQueryOutputModel SearchQuery(RecordQueryInputModel input)
        {
            var query = from r in RecordQuery(input)
                        let titleFragments = titleLites.GetFragments("records/" + r.Id).Concat(titleNLites.GetFragments("records/" + r.Id))
                        let abstractFragments = abstractLites.GetFragments("records/" + r.Id).Concat(abstractNLites.GetFragments("records/" + r.Id))
                        let title = titleFragments.Select(f => f.TruncateNicely(200)).FirstOrDefault()
                                    ?? r.Gemini.Title.TruncateNicely(200)
                        let snippet = abstractFragments.Select(f => f.TruncateNicely(200)).FirstOrDefault()
                                       ?? r.Gemini.Abstract.TruncateNicely(200)
                        let format = DataFormatQueries.GetDataFormatInfo(r.Gemini.DataFormat)
                        select new ResultOutputModel
                            {
                                Id = r.Id,
                                Title = title, // could be better; always want the whole title, highlighted
                                Snippet = snippet,
                                Format = new FormatOutputModel
                                    {
                                        Group = format.Group,
                                        Glyph = format.Glyph,
                                        Name = format.Name,
                                    },
                                Keywords = r.Gemini.Keywords,
                                            //.OrderBy(k => k.Vocab != "http://vocab.jncc.gov.uk/jncc-broad-category") // show first
                                            //.ThenBy(k => k.Vocab.IsBlank())
                                            //.ThenBy(k => k.Vocab).ToList(),
                                TopCopy = r.TopCopy,
                                Date = r.Gemini.DatasetReferenceDate,
                                ResourceType = r.Gemini.ResourceType.FirstCharToUpper(),
                                Box = r.Gemini.BoundingBox,
                            };

            // materializing the query will populate our stats, so do it before we try to use them!
            var results = query.ToList();

            return new RecordQueryOutputModel
            {
                Total = stats.TotalResults,
                Results = results,
                Speed = stats.DurationMilliseconds,
                Query = input
            };
        }
    }
}
