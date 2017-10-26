using Catalogue.Data.Indexes;
using Raven.Client;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Catalogue.Data.Query
{
    public static class QueryExtensions
    {
        public static IQueryable<RecordIndex.Result> SearchMultiple(this IQueryable<RecordIndex.Result> query,
            Expression<Func<RecordIndex.Result, object>> expression, string terms, int boost, SearchOptions option)
        {
            var reg = new Regex("[^\\s\"']+|\"[^\"]*\"|'[^']*'"); //Splits string by spaces, groups words into phrases if surrounded by quotes
            var searchTerms = reg.Matches(terms).Cast<Match>().Select(m => m.Value).ToList();

            foreach (var term in searchTerms)
            {
                query = query.Search(expression, term, boost, option, EscapeQueryOptions.RawQuery);
            }
            return query;
        }
    }
}
