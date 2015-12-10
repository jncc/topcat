using System;
using System.Linq;
using Catalogue.Data.Model;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class RecordsForPublishingIndex : AbstractIndexCreationTask<Record, RecordsForPublishingIndex.Result>
    {
        public class Result
        {
            //public TimeSpan Staleness   { get; set; }
            public DateTime LastSuccess { get; set; }
            public DateTime MetadataDate { get; set; }
        }

        /// <summary>
        /// Calculates a value which is the length of time between the last publication and when the record was last edited.
        /// If this is a positive number, the record is "stale" with respect to publication, so needs publishing.
        /// </summary>
        public RecordsForPublishingIndex()
        {
            Map = records => from r in records
                             where r.Publication != null
                             where r.Publication.OpenData != null
                             select new
                             {
                                 LastSuccess = r.Gemini.MetadataDate, // r.Publication.OpenData.Attempts.Last().DateUtc,
                                 MetadataDate = r.Gemini.MetadataDate
                                 //Staleness = r.Gemini.MetadataDate - r.Publication.LastSuccess
                             };
        }
    }
}
