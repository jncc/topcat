using System;
using Catalogue.Gemini.Model;

namespace Catalogue.Data.Model
{
    public class Record
    {
        public Guid Id { get; set; }

        public Metadata Metadata { get; set; }

        public string   Wkt      { get; set; }

        public Status   Status   { get; set; }
        public bool     TopCopy  { get; set; }
        public string   Notes    { get; set; }
    }
}
