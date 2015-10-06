using System;
using System.Collections.Generic;
using Catalogue.Gemini.Model;

namespace Catalogue.Data.Query
{
    public class RecordQueryOutputModel
    {
        public int Total { get; set; }
        public List<ResultOutputModel> Results { get; set; }
        public long Speed { get; set; }

        public RecordQueryInputModel Query { get; set; }
    }

    public class ResultOutputModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public FormatOutputModel Format { get; set; }
        public string Snippet { get; set; }
        public List<MetadataKeywordOutputModel> Keywords { get; set; }
        public bool TopCopy { get; set; }
        public string Date { get; set; }
        public string ResourceType { get; set; }

        public string TemporalExtentFrom { get; set; }
        public string TemporalExtentTo { get; set; }

        public BoundingBox Box { get; set; }
    }

    public class FormatOutputModel
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string Glyph { get; set; }
    }

    public class MetadataKeywordOutputModel
    {
        public string Value { get; set; }
        public string Vocab { get; set; }
        public bool Squash { get; set; }
    }
}
