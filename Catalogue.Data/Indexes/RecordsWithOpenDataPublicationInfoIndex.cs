﻿using System;
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
                            .Where(r => r.Publication != null)
                            .Select(r => new Result
                            {
                                RecordLastUpdatedDate = r.Gemini.MetadataDate,
                                LastPublicationAttemptDate = r.Publication.Gov != null && r.Publication.Gov.LastAttempt != null ? r.Publication.Gov.LastAttempt.DateUtc : DateTime.MinValue,
                                LastSuccessfulPublicationAttemptDate = r.Publication.Gov == null || r.Publication.Gov.LastSuccess == null ?
                                    DateTime.MinValue : r.Publication.Gov.LastSuccess.DateUtc,
                                GeminiValidated = r.Validation == Validation.Gemini,
                                Assessed = r.Publication.Assessment != null && r.Publication.Assessment.Completed
                                           && (r.Publication.Assessment.CompletedOnUtc == r.Gemini.MetadataDate
                                               || r.Publication.SignOff != null && r.Publication.SignOff.DateUtc == r.Gemini.MetadataDate
                                               || r.Publication.Gov != null && r.Publication.Gov.LastAttempt != null && r.Publication.Gov.LastAttempt.DateUtc == r.Gemini.MetadataDate),
                                SignedOff = r.Publication.SignOff != null && r.Publication.SignOff.DateUtc == r.Gemini.MetadataDate
                                            || r.Publication.Gov != null && r.Publication.Gov.LastAttempt.DateUtc == r.Gemini.MetadataDate,
                                PublicationNeverAttempted = r.Publication.Gov == null || r.Publication.Gov.LastAttempt == null && r.Publication.SignOff.DateUtc == r.Gemini.MetadataDate,
                                LastPublicationAttemptWasUnsuccessful = r.Publication.Gov != null && r.Publication.Gov.LastAttempt != null && r.Publication.Gov.LastSuccess == null
                                                                        || r.Publication.Gov.LastAttempt != null && r.Publication.Gov.LastSuccess != null
                                                                        && r.Publication.Gov.LastAttempt.DateUtc > r.Publication.Gov.LastSuccess.DateUtc,
                                PublishedSinceLastUpdated = r.Publication.Gov.LastSuccess.DateUtc >= r.Gemini.MetadataDate,
                                PublishingIsPaused = r.Publication.Gov.Paused
                            });
        }
    }
}
