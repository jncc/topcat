using System;
using System.Linq;
using Catalogue.Data.Model;
using Raven.Client.Indexes;
using static Catalogue.Data.Model.RecordEvent;

namespace Catalogue.Data.Indexes
{
    public class RecordEventIndex : AbstractIndexCreationTask<Record, RecordEventIndex.Result>
    {
        public class Result
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public DateTime Date { get; set; }
            public string User { get; set; }
            public RecordEvent Event { get; set; }
        }

        public RecordEventIndex()
        {
            Map = records => from r in records
                where r.Footer == null
                orderby r.Footer.ModifiedOnUtc
                select new Result
                {
                    Id = r.Id,
                    Title = r.Gemini.Title,
                    Date = r.Gemini.MetadataDate,
                    User = r.Footer.ModifiedBy,
                    Event = r.Footer.CreatedOnUtc.Equals(r.Footer.ModifiedOnUtc) ? Create : Edit
                };
        }
    }
}
