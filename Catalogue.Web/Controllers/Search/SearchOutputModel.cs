using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchOutputModel
    {
        public int Total { get; set; }
        public List<ResultOutputModel> Results { get; set; }
        public long Speed { get; set; }

        public QueryOutputModel Query { get; set; }
    }

    public class ResultOutputModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public FormatOutputModel Format { get; set; }
        public string Snippet { get; set; }
        public List<Keyword> Keywords { get; set; }
        public bool TopCopy { get; set; }
        public string Date { get; set; }
        public string TemporalExtentFrom { get; set; }
        public string TemporalExtentTo { get; set; }
    }

    public class QueryOutputModel
    {
        public string Q { get; set; }
        public int    P { get; set; }
    }

    public class FormatOutputModel
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string Glyph { get; set; }
    }
}
