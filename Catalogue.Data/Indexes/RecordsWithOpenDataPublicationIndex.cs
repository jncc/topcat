using System;
using System.Linq;
using Catalogue.Data.Model;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class RecordsWithOpenDataPublicationIndex : AbstractIndexCreationTask<Record, RecordsWithOpenDataPublicationIndex.Result>
    {
        public class Result
        {
            public DateTime MetadataDate { get; set; }
            public DateTime LastPublicationAttemptDate { get; set; }
            public DateTime LastSuccessfulPublicationAttemptDate { get; set; }
        }

        public RecordsWithOpenDataPublicationIndex()
        {
            Map = records => from r in records
                             where r.Publication != null
                             where r.Publication.OpenData != null
                             select new
                             {
                                 LastPublicationAttemptDate = r.Publication.OpenData.LastAttempt == null ? DateTime.MinValue : r.Publication.OpenData.LastAttempt.DateUtc,
                                 LastSuccessfulOpenDataPublicationAttemptDate = r.Publication.OpenData.LastSuccessfulAttempt == null ? DateTime.MinValue : r.Publication.OpenData.LastSuccessfulAttempt.DateUtc,
                                 MetadataDate = r.Gemini.MetadataDate
                             };
        }
    }
}
