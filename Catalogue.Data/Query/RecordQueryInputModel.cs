using System;
using System.Linq;
using Catalogue.Utilities.Text;

namespace Catalogue.Data.Query
{
    public class RecordQueryInputModel
    {
        private string q;

        public RecordQueryInputModel()
        {
            N = 15;
            P = 0;
            Q = String.Empty;
            K = new string[0];
        }

        /// <summary>
        /// The number of records (page size).
        /// </summary>
        public int N { get; set; }

        /// <summary>
        /// The page number.
        /// </summary>
        public int P { get; set; }

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

        public bool HasKeywords()
        {
            return this.K != null && this.K.Any() && this.K.First().IsNotBlank();
        }
    }

}
