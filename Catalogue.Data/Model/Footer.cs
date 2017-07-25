using System;

namespace Catalogue.Data.Model
{
    public class Footer
    {
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOnUtc { get; set; }
        public string ModifiedBy { get; set; }
    }
}
