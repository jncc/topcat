using System;

namespace Catalogue.Data.Model
{
    public class Footer
    {
        public DateTime CreatedOnUtc { get; set; } // DateTime.MinValue
        public string CreatedBy { get; set; } // Joint Nature Conservation Committee
        public DateTime ModifiedOnUtc { get; set; } // MetadataDate
        public string ModifiedBy { get; set; } // Meta user .? Joint Nature Conservation Committee
    }
}
