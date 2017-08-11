using System;

namespace Catalogue.Data.Model
{
    public class Footer
    {
        public DateTime CreatedOnUtc { get; set; }
        public UserInfo CreatedByUser { get; set; }
        public DateTime ModifiedOnUtc { get; set; }
        public UserInfo ModifiedByUser { get; set; }
    }
}
