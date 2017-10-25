using Catalogue.Data.Indexes;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Raven.Client;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Version = Lucene.Net.Util.Version;

namespace Catalogue.Data.Query
{
    public static class QueryExtensions
    {
        public static IQueryable<RecordIndex.Result> SearchMultiple(this IQueryable<RecordIndex.Result> query,
            Expression<Func<RecordIndex.Result, object>> expression, string terms, int boost, SearchOptions option)
        {
            var parser = new QueryParser(Version.LUCENE_30, "content", new StandardAnalyzer(Version.LUCENE_30));
            var parsedTerms = parser.Parse(terms);
            var reg = new Regex("(?<=\")[^\"]*(?=\")|[^\" ]+");
            var searchTerms = reg.Matches(terms).Cast<Match>().Select(m => m.Value).ToList();

            foreach (var term in searchTerms)
            {
                query = query.Search(expression, term, boost, option, EscapeQueryOptions.RawQuery);
            }
            return query;
        }
    }
}
