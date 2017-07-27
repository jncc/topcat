using System;
using System.Collections.Generic;

namespace Catalogue.Web.Controllers.Usage
{
    public class UsageOutputModel
    {
        public List<RecentlyModifiedRecord> RecentlyModifiedRecords { get; set; }
    }

    public class RecentlyModifiedRecord
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
        public string Event { get; set; }
    }
}