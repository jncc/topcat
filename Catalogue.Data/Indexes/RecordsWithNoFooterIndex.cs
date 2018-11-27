using System;
using System.Linq;
using Catalogue.Data.Model;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Data.Indexes
{
    public class RecordsWithNoFooterIndex : AbstractIndexCreationTask<Record, RecordsWithNoFooterIndex.Result>
    {
        public class Result
        {
            public string MetadataPointOfContactName { get; set; }
            public DateTime MetadataDate { get; set; }
        }

        public RecordsWithNoFooterIndex()
        {
            Map = records => from r in records
                where r.Footer == null
                select new Result
                {
                    MetadataPointOfContactName = r.Gemini.MetadataPointOfContact.Name,
                    MetadataDate = r.Gemini.MetadataDate
                };
        }
    }
}
