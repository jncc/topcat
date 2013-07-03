using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalogue.Data.Model
{
    public class Item
    {
        public Guid Id { get; set; }

        public string   Xml      { get; set; }
        public Metadata Metadata { get; set; }

        public string   Location { get; set; }
        public bool     TopCopy  { get; set; }
    }


    public class Metadata
    {
        public string Title { get; set; }
        public string DataFormat { get; set; }
        public string BoundingBox { get; set; }
    }
}
