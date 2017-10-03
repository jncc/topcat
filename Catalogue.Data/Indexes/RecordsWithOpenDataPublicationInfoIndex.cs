﻿using System;
using System.Linq;
using Catalogue.Data.Model;
using Raven.Client.Indexes;

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

            Map = records => from r in records
                             where r.Publication != null
                             where r.Publication.OpenData != null
                             let assessed = r.Publication.OpenData.Assessment.Completed
                                && (r.Publication.OpenData.Assessment.CompletedOnUtc == r.Gemini.MetadataDate
                                || r.Publication.OpenData.SignOff.DateUtc == r.Gemini.MetadataDate
                                || r.Publication.OpenData.LastAttempt.DateUtc == r.Gemini.MetadataDate)
                             let signedOff = r.Publication.OpenData.SignOff.DateUtc == r.Gemini.MetadataDate
                                || r.Publication.OpenData.LastAttempt.DateUtc == r.Gemini.MetadataDate
                             let recordLastUpdatedDate = r.Gemini.MetadataDate
                             let lastAttemptDate = r.Publication.OpenData.LastAttempt == null ? DateTime.MinValue : r.Publication.OpenData.LastAttempt.DateUtc
                             let lastSuccessDate = r.Publication.OpenData.LastSuccess == null ? DateTime.MinValue : r.Publication.OpenData.LastSuccess.DateUtc
                             let neverAttempted = lastAttemptDate == DateTime.MinValue && r.Publication.OpenData.SignOff != null
                             let publishedSinceLastUpdated = r.Publication.OpenData.LastSuccess.DateUtc == r.Gemini.MetadataDate

                             select new Result
                             {
                                 RecordLastUpdatedDate = recordLastUpdatedDate,
                                 LastPublicationAttemptDate = lastAttemptDate,
                                 LastSuccessfulPublicationAttemptDate = lastSuccessDate,
                                 GeminiValidated = r.Validation == Validation.Gemini,
                                 Assessed = assessed,
                                 SignedOff = signedOff,
                                 PublicationNeverAttempted = neverAttempted,
                                 LastPublicationAttemptWasUnsuccessful = lastAttemptDate > lastSuccessDate,
                                 PublishedSinceLastUpdated = publishedSinceLastUpdated,
                                 PublishingIsPaused = r.Publication.OpenData.Paused,
                             };
        }
    }
}
