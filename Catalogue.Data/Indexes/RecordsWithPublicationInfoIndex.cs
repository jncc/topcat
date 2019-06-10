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
            public bool GeminiValidated { get; set; }
            public bool HubPublishable { get; set; }
            public bool GovPublishable { get; set; }
            public bool Assessed { get; set; }
            public bool SignedOff { get; set; }
            public bool PublicationNeverAttempted { get; set; }
            public bool PublishedToHubSinceLastUpdated { get; set; }
            public bool PublishedToGovSinceLastUpdated { get; set; }
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
                                GeminiValidated = r.Validation == Validation.Gemini,
                                HubPublishable = r.Publication.Target.Hub != null && r.Publication.Target.Hub.Publishable == true,
                                GovPublishable = r.Publication.Target.Gov != null && r.Publication.Target.Gov.Publishable == true,
                                Assessed = r.Publication.Assessment != null && r.Publication.Assessment.Completed
                                           && (r.Publication.Assessment.CompletedOnUtc == r.Gemini.MetadataDate
                                               || r.Publication.SignOff != null && r.Publication.SignOff.DateUtc == r.Gemini.MetadataDate
                                               || r.Publication.Data != null && r.Publication.Data.LastAttempt != null && r.Publication.Data.LastAttempt.DateUtc == r.Gemini.MetadataDate
                                               || r.Publication.Target.Hub != null && r.Publication.Target.Hub.LastAttempt != null && r.Publication.Target.Hub.LastAttempt.DateUtc == r.Gemini.MetadataDate
                                               || r.Publication.Target.Gov != null && r.Publication.Target.Gov.LastAttempt != null && r.Publication.Target.Gov.LastAttempt.DateUtc == r.Gemini.MetadataDate),
                                SignedOff = r.Publication.SignOff != null && r.Publication.SignOff.DateUtc == r.Gemini.MetadataDate
                                            || r.Publication.Data != null && r.Publication.Data.LastAttempt != null && r.Publication.Data.LastAttempt.DateUtc == r.Gemini.MetadataDate
                                            || r.Publication.Target.Hub != null && r.Publication.Target.Hub.LastAttempt != null && r.Publication.Target.Hub.LastAttempt.DateUtc == r.Gemini.MetadataDate
                                            || r.Publication.Target.Gov != null && r.Publication.Target.Gov.LastAttempt != null && r.Publication.Target.Gov.LastAttempt.DateUtc == r.Gemini.MetadataDate,
                                PublicationNeverAttempted = r.Publication.Target.Hub == null && r.Publication.Target.Gov == null ||
                                                            r.Publication.Target.Hub.LastAttempt == null && r.Publication.Target.Gov.LastAttempt == null &&
                                                            r.Publication.SignOff != null && r.Publication.SignOff.DateUtc == r.Gemini.MetadataDate,
                                PublishedToHubSinceLastUpdated = r.Publication.Target.Hub != null && r.Publication.Target.Hub.LastSuccess != null &&
                                                                 (r.Publication.Target.Hub.LastSuccess.DateUtc >= r.Gemini.MetadataDate ||
                                                                 r.Publication.SignOff != null && r.Publication.Target.Hub.LastSuccess.DateUtc > r.Publication.SignOff.DateUtc),
                                PublishedToGovSinceLastUpdated = r.Publication.Target.Gov != null &&
                                                                 r.Publication.Target.Gov.LastSuccess != null && r.Publication.Target.Gov.LastSuccess.DateUtc >= r.Gemini.MetadataDate
                            });
        }
    }
}
