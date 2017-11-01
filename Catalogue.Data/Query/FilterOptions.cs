using System;

namespace Catalogue.Data.Query
{
    public class FilterOptions
    {
        /// <summary>
        /// The keywords to restrict the query to.
        /// </summary>
        public string[] Keywords { get; set; }

        /// <summary>
        /// The data formats to restrict the query to.
        /// </summary>
        public string[] DataFormats { get; set; }

        /// <summary>
        /// The earliest metadata date from which records should be returned.
        /// </summary>
        public DateTime? MetadataDate { get; set; }
    }
}
