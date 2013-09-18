using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchOutputModel
    {
        public int Total { get; set; }
        public List<ResultOutputModel> Results { get; set; }
        public long Speed { get; set; }

        public string Query { get; set; }
    }

    public class ResultOutputModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
    }
}
