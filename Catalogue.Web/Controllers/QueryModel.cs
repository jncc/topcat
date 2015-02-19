using System;
using System.Linq;
using Catalogue.Utilities.Text;

namespace Catalogue.Web.Controllers
{
    public class QueryModel
    {
        public QueryModel()
        {
            N = 25;
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
        public string Q { get; set; }

        /// <summary>
        /// The keywords to restrict the query to.
        /// </summary>
        public string[] K { get; set; }

        public bool HasKeywords
        {
            get { return this.K.Any() && this.K.First().IsNotBlank(); }
        }
    }

}