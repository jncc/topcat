using System;
using System.Collections.Generic;
using System.Linq;
namespace Catalogue.Utilities.Raven
{
    public static class RavenUtilities
    {
        /// <summary>
        /// Safely retrieve results up to a sensible specified maximum page size,
        /// and receive an exception if there are more.
        /// Rationale: earlier versions of RavenDB silently limit the number of results
        /// by design, which can lead to incorrect assumptions and false results.
        /// </summary>
        public static IReadOnlyList<T> Fetch<T>(this IQueryable<T> source, int max)
        {
            var results = source.Take(max + 1).ToList();
            
            if (results.Count > max)
                throw new Exception("More than " + max + " results. Redesign the UI or page the results.");

            return results;
        }
    }
}
