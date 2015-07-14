using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Catalogue.Utilities.Text;

namespace Catalogue.Web.Controllers
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
        }
        /// <summary>
        /// The earliest last modified date of the records to search, or null exclude from search
        /// </summary>
        public DateTime? D { get; set; }

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
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Q { get; set; }

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
