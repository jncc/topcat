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
    public static class AsyncRecordQuerier
    {

        /// <summary>
        /// A general-purpose query that returns records.
        /// Can be materialised as-is, or customised further (see SearchQuery method).
        /// We may need to refactor this to support ravendb streaming for larger result sets.
        /// </summary>
        public static IQueryable<Record> RecordQuery(IAsyncDocumentSession db, RecordQueryInputModel input)
        {
            RavenQueryStatistics stats;

            var query = db.Query<RecordIndex.Result, RecordIndex>()
                .Statistics(out stats)
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

            if (input.D != null)
            {
                query = query.Where(r => r.MetadataDate >= input.D);
            }

            return query.As<Record>()
                // ravendb method to project from the index result type to the actual document type
                .Skip(input.P*input.N)
                .Take(input.N);
        }
    }
}