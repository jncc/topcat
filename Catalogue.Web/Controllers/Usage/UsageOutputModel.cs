using System;
using Catalogue.Data.Model;
using System.Collections.Generic;

namespace Catalogue.Web.Controllers.Usage
{
    public class UsageOutputModel
    {
        public List<ModifiedRecord> RecentlyModifiedRecords { get; set; }
    }

    public class ModifiedRecord
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
    }
}