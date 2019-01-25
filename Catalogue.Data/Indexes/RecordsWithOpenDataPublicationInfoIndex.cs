using System;
using System.Linq;
using System.Linq.Expressions;
using Catalogue.Data.Model;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Data.Indexes
{
    public class RecordsWithOpenDataPublicationInfoIndex : AbstractIndexCreationTask<Record, RecordsWithOpenDataPublicationInfoIndex.Result>
    {
        public class Result
        {
            public DateTime RecordLastUpdatedDate { get; set; }
            public DateTime LastPublicationAttemptDate { get; set; }
            public DateTime LastSuccessfulPublicationAttemptDate { get; set; }
            public bool GeminiValidated { get; set; }
            public bool? Publishable { get; set; }
            public bool Assessed { get; set; }
            public bool SignedOff { get; set; }
            public bool PublicationNeverAttempted { get; set; }
            public bool LastPublicationAttemptWasUnsuccessful { get; set; }
            public bool PublishedSinceLastUpdated { get; set; }
            public bool PublishingIsPaused { get; set; }
        }

        public RecordsWithOpenDataPublicationInfoIndex()
        {
            // note that these calculations rely on the trick of using
            // DateTime.MinValue to avoid nulls and enable simple value comparisions which can be done on the RavenDB server
            Map = records => records
                            .Where(r => r.Publication != null && r.Publication.Gov != null)
                            .Select(r => new Result
                            {
                                RecordLastUpdatedDate = r.Gemini.MetadataDate,
                                LastPublicationAttemptDate = r.Publication.Gov.LastAttempt == null ? DateTime.MinValue : r.Publication.Gov.LastAttempt.DateUtc,
                                LastSuccessfulPublicationAttemptDate = r.Publication.Gov.LastSuccess == null ? DateTime.MinValue : r.Publication.Gov.LastSuccess.DateUtc,
                                GeminiValidated = r.Validation == Validation.Gemini,
                                Publishable = r.Publication.Gov.Publishable,
                                Assessed = r.Publication.Gov.Assessment.Completed
                                           && (r.Publication.Gov.Assessment.CompletedOnUtc == r.Gemini.MetadataDate
                                               || r.Publication.Gov.SignOff.DateUtc == r.Gemini.MetadataDate
                                               || r.Publication.Gov.LastAttempt.DateUtc == r.Gemini.MetadataDate),
                                SignedOff = r.Publication.Gov.SignOff.DateUtc == r.Gemini.MetadataDate
                                            || r.Publication.Gov.LastAttempt.DateUtc == r.Gemini.MetadataDate,
                                PublicationNeverAttempted = r.Publication.Gov.LastAttempt == null && r.Publication.Gov.SignOff.DateUtc == r.Gemini.MetadataDate,
                                LastPublicationAttemptWasUnsuccessful = (r.Publication.Gov.LastAttempt != null && r.Publication.Gov.LastSuccess == null)
                                                                        || r.Publication.Gov.LastAttempt != null && r.Publication.Gov.LastSuccess != null
                                                                        && r.Publication.Gov.LastAttempt.DateUtc > r.Publication.Gov.LastSuccess.DateUtc,
                                PublishedSinceLastUpdated = r.Publication.Gov.LastSuccess.DateUtc >= r.Gemini.MetadataDate,
                                PublishingIsPaused = r.Publication.Gov.Paused
                            });
        }
    }
}
