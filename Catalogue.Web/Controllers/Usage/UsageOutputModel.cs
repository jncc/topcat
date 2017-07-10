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

        public override bool Equals(object obj)
        {
            var item = obj as ModifiedRecord;

            if (item == null)
            {
                return false;
            }

            return Id.Equals(item.Id)
                   && Title.Equals(item.Title)
                   && Date.Equals(item.Date)
                   && User.Equals(item.User);
        }

        public override int GetHashCode()
        {
            return new {Id, Title, Date, User}.GetHashCode();
        }
    }
}