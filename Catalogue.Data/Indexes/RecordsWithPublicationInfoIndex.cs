using Catalogue.Data.Model;
using Raven.Client.Documents.Indexes;
using System;
using System.Linq;

namespace Catalogue.Data.Indexes
{
    public class RecordsWithPublicationInfoIndex : AbstractIndexCreationTask<Record, RecordsWithPublicationInfoIndex.Result>
    {
        public class Result
        {
            public DateTime RecordLastUpdatedDate { get; set; }
            public DateTime LastPublicationAttemptDate { get; set; }
            public DateTime LastSuccessfulPublicationAttemptDate { get; set; }
            public bool GeminiValidated { get; set; }
            public bool Assessed { get; set; }
            public bool SignedOff { get; set; }
            public bool PublicationNeverAttempted { get; set; }
            public bool LastPublicationAttemptWasUnsuccessful { get; set; }
            public bool PublishedToHubSinceLastUpdated { get; set; }
            public bool PublishedToGovSinceLastUpdated { get; set; }
            public bool GovPublishingIsPaused { get; set; }
        }

        public RecordsWithPublicationInfoIndex()
        {
            // note that these calculations rely on the trick of using
            // DateTime.MinValue to avoid nulls and enable simple value comparisions which can be done on the RavenDB server
            Map = records => records
                            .Where(r => r.Publication != null && r.Publication.Target != null)
                            .Select(r => new Result
                            {
                                RecordLastUpdatedDate = r.Gemini.MetadataDate,
                                LastPublicationAttemptDate = r.Publication.Target.Gov != null && r.Publication.Target.Gov.LastAttempt != null ? r.Publication.Target.Gov.LastAttempt.DateUtc : DateTime.MinValue,
                                LastSuccessfulPublicationAttemptDate = r.Publication.Target.Gov == null || r.Publication.Target.Gov.LastSuccess == null ?
                                    DateTime.MinValue : r.Publication.Target.Gov.LastSuccess.DateUtc,
                                GeminiValidated = r.Validation == Validation.Gemini,
                                Assessed = r.Publication.Assessment != null && r.Publication.Assessment.Completed
                                           && (r.Publication.Assessment.CompletedOnUtc == r.Gemini.MetadataDate
                                               || r.Publication.SignOff != null && r.Publication.SignOff.DateUtc == r.Gemini.MetadataDate
                                               || r.Publication.Target.Gov != null && r.Publication.Target.Gov.LastAttempt != null && r.Publication.Target.Gov.LastAttempt.DateUtc == r.Gemini.MetadataDate),
                                SignedOff = r.Publication.SignOff != null && r.Publication.SignOff.DateUtc == r.Gemini.MetadataDate
                                            || r.Publication.Target.Gov != null && r.Publication.Target.Gov.LastAttempt.DateUtc == r.Gemini.MetadataDate,
                                PublicationNeverAttempted = r.Publication.Target.Gov == null || r.Publication.Target.Gov.LastAttempt == null && r.Publication.SignOff.DateUtc == r.Gemini.MetadataDate,
                                LastPublicationAttemptWasUnsuccessful = r.Publication.Target.Gov != null && r.Publication.Target.Gov.LastAttempt != null && r.Publication.Target.Gov.LastSuccess == null
                                                                        || r.Publication.Target.Gov.LastAttempt != null && r.Publication.Target.Gov.LastSuccess != null
                                                                        && r.Publication.Target.Gov.LastAttempt.DateUtc > r.Publication.Target.Gov.LastSuccess.DateUtc,
                                PublishedToHubSinceLastUpdated = r.Publication.Target.Hub.LastSuccess.DateUtc >= r.Gemini.MetadataDate,
                                PublishedToGovSinceLastUpdated = r.Publication.Target.Gov.LastSuccess.DateUtc >= r.Gemini.MetadataDate,
                                GovPublishingIsPaused = r.Publication.Target.Gov.Paused
                            });
        }
    }
}
