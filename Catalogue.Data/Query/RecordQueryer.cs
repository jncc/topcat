using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Gemini.DataFormats;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using Raven.Client;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;

namespace Catalogue.Data.Query
{
    public interface IRecordQueryer
    {
        SearchOutputModel Search(RecordQueryInputModel input);

        IQueryable<Record> Query(RecordQueryInputModel input);
        IQueryable<Record> AsyncQuery(IAsyncDocumentSession adb, RecordQueryInputModel input);
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

        public IQueryable<Record> AsyncQuery(IAsyncDocumentSession adb, RecordQueryInputModel input)
        {
            var query = adb.Query<RecordIndex.Result, RecordIndex>()
                .Statistics(out stats);

            return RecordQueryImpl(input, query);
        }


        /// <summary>
        /// A general-purpose query that returns records.
        /// Can be materialised as-is, or customised further (see SearchQuery method).
        /// </summary>
        public IQueryable<Record> Query(RecordQueryInputModel input)
        {

            var query = _db.Query<RecordIndex.Result, RecordIndex>()
                .Statistics(out stats)
                .Customize(x => x.Highlight("Title", 202, 1, out titleLites))
                .Customize(x => x.Highlight("TitleN", 202, 1, out titleNLites))
                .Customize(x => x.Highlight("Abstract", 202, 1, out abstractLites))
                .Customize(x => x.Highlight("AbstractN", 202, 1, out abstractNLites))
                .Customize(x => x.SetHighlighterTags("<b>", "</b>"));

            return RecordQueryImpl(input, query);

        }

        IQueryable<Record> RecordQueryImpl(RecordQueryInputModel input, IQueryable<RecordIndex.Result> query)
        {
            if (input.Q.IsNotBlank())
            {
                query = query
                    .Search(r => r.Title, input.Q, boost: 10)
                    .Search(r => r.TitleN, input.Q)
                    .Search(r => r.Abstract, input.Q)
                    .Search(r => r.AbstractN, input.Q)
                    .Search(r => r.KeywordsN, input.Q);
            }

            if (input.K != null && input.K.Any())
            {
                foreach (var keyword in ParameterHelper.ParseMetadataKeywords(input.K))
                {
                    string k = keyword.Vocab + "/" + keyword.Value;
                    query = query.Where(r => r.Keywords.Contains(k));
                }
            }

            if (input.D != null)
            {
                query = query.Where(r => r.MetadataDate >= input.D);
            }

            var recordQuery = query.As<Record>(); // ravendb method to project from the index result type to the actual document type

            recordQuery = SortRecords(recordQuery, input);

            // allow N to be negative
            if (input.N >= 0)
            {
                recordQuery = recordQuery.Skip(input.P * input.N).Take(input.N);
            }


            return recordQuery;
        }



        /// <summary>
        /// A query for the Google-style search results page.
        /// </summary>
        public SearchOutputModel Search(RecordQueryInputModel input)
        {
            // materializing the query will populate our stats

            var results = from r in Query(input).ToList()
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
                            Keywords = MakeKeywordOutputModelList(r.Gemini.Keywords).ToList(),
                            //.OrderBy(k => k.Vocab != "http://vocab.jncc.gov.uk/jncc-broad-category") // show first
                            //.ThenBy(k => k.Vocab.IsBlank())
                            //.ThenBy(k => k.Vocab).ToList(),
                            TopCopy = r.TopCopy,
                            Date = r.Gemini.DatasetReferenceDate,
                            ResourceType = r.Gemini.ResourceType.FirstCharToUpper(),
                            Box = r.Gemini.BoundingBox,
                        };


            return new SearchOutputModel
            {
                Total = stats.TotalResults,
                Results = results.ToList(),
                Speed = stats.DurationMilliseconds,
                Query = input
            };
        }

        IEnumerable<MetadataKeywordOutputModel> MakeKeywordOutputModelList(List<MetadataKeyword> keywords)
        {
            // the MetadataKeywordOutputModel has an additional property "Squash"
            // to tell the consumer (UI) that the keyword may be bunched up to save space
            // (when there are many keywords in the same vocab)

            var output = from k in keywords
                         group k by k.Vocab into g
                         let groupIsBig = g.Count() >= 5 // we'll squash groups of 5 or more
                         from k in g
                         let keywordIsLastInGroup = k == g.Last() // we don't squash the last keyword in the group
                         select new MetadataKeywordOutputModel
                         {
                             Vocab = k.Vocab,
                             Value = k.Value,
                             Squash = groupIsBig && !keywordIsLastInGroup,
                         };

            return output;
        }

        private IQueryable<Record> SortRecords(IQueryable<Record> recordQuery, RecordQueryInputModel input)
        {
            var sortedRecords = recordQuery;
            Expression<Func<Record, Object>> orderByFunc;
            switch (input.O)
            {
                case 0:
                    break;
                case 1:
                    orderByFunc = record => record.Gemini.Title;
                    sortedRecords = recordQuery.OrderBy(orderByFunc);
                    break;
                case 2:
                    orderByFunc = record => record.Gemini.Title;
                    sortedRecords = recordQuery.OrderByDescending(orderByFunc);
                    break;
                case 3:
                    orderByFunc = record => record.Gemini.DatasetReferenceDate;
                    sortedRecords = recordQuery.OrderByDescending(orderByFunc);
                    break;
                case 4:
                    orderByFunc = record => record.Gemini.DatasetReferenceDate;
                    sortedRecords = recordQuery.OrderBy(orderByFunc);
                    break;
            }

            return sortedRecords;
        }
    }
}
