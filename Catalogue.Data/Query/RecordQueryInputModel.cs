using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Catalogue.Utilities.Text;

namespace Catalogue.Data.Query
{
    public class RecordQueryInputModel
    {
        public RecordQueryInputModel()
        {
            N = 15;
            P = 0;
            Q = String.Empty;
            K = new string[0];
            D = null;
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
        /// The keywords to restrict the query to.
        /// </summary>
        public string[] K { get; set; }

        /// <summary>
        /// The earliest metadata date from which records should be returned.
        /// </summary>
        public DateTime? D { get; set; }

        /// <summary>
        /// The sort option for results.
        /// 0 - Relevance
        /// 1 - Title desc
        /// 2 - Title asc
        /// 3 - Dataset reference date desc
        /// 4 - Dataset reference date asc
        /// </summary>
        public int O { get; set; }
    }
}
