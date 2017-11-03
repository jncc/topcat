using System;

namespace Catalogue.Data.Query
{
    public class RecordQueryInputModel
    {
        public RecordQueryInputModel()
        {
            N = 15;
            P = 0;
            Q = String.Empty;
            F = null;
            O = 0;
        }

        /// <summary>
        /// The number of records (page size). Use a negative value for no paging.
        /// </summary>
        public int N { get; set; }

        /// <summary>
        /// The page number.
        /// </summary>
        public int P { get; set; }

        string q;

        /// <summary>
        /// The full-text search query.
        /// </summary>
        public string Q
        {
            get { return q ?? String.Empty; } // makes things easier on the client
            set { q = value; }
        }

        /// <summary>
        /// The criteria to filter the results by.
        /// </summary>
        public FilterOptions F { get; set; }

        /// <summary>
        /// The sort option for results.
        /// 0 - Relevance
        /// 1 - Title desc
        /// 2 - Title asc
        /// 3 - Dataset reference date desc
        /// 4 - Dataset reference date asc
        /// </summary>
        public SortOptions O { get; set; }
    }
}
