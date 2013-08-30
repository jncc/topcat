using System;
using Catalogue.Gemini.Model;

namespace Catalogue.Data.Model
{
    public class Item
    {
        public Guid Id { get; set; }

        public string   Xml      { get; set; }
        public Metadata Metadata { get; set; }

        public string   Wkt      { get; set; }

        public bool     Public   { get; set; }
        public string   Location { get; set; }
        public string   Notes    { get; set; }
        public bool     TopCopy  { get; set; }
    }
}
