using System;
using System.Linq;
using System.Linq.Expressions;
using Catalogue.Data.Indexes;
using Raven.Client;

namespace Catalogue.Data.Query
{
    public static class QueryExtensions
    {
        public static IQueryable<RecordIndex.Result> SearchMultiple(this IQueryable<RecordIndex.Result> query,
            Expression<Func<RecordIndex.Result, object>> expr, string input, int boost, SearchOptions searchOption)
        {
            var terms = input.Split(' ');

            foreach (var term in terms)
            {
                query = query.Search(expr, term, boost, searchOption);
            }

            return query;
        }
    }
}