﻿using System;
using Catalogue.Data.Model;

namespace Catalogue.Data.Extensions
{
    public static class RecordExtensions
    {

        public static bool IsAssessedAndUpToDate(this Record record)
        {
            return record.Publication?.Assessment != null
                && record.Publication.Assessment.Completed
                && (record.Publication.Assessment.CompletedOnUtc.Equals(record.Gemini.MetadataDate)
                || IsSignedOffAndUpToDate(record));
        }

        public static bool IsSignedOffAndUpToDate(this Record record)
        {
            return record.Publication?.SignOff != null
                && (record.Publication.SignOff.DateUtc.Equals(record.Gemini.MetadataDate)
                || record.Publication.Data?.LastAttempt != null
                && record.Publication.Data.LastAttempt.DateUtc.Equals(record.Gemini.MetadataDate)
                || record.Publication.Hub?.LastAttempt != null
                && record.Publication.Hub.LastAttempt.DateUtc.Equals(record.Gemini.MetadataDate)
                || record.Publication.Gov?.LastAttempt != null
                && record.Publication.Gov.LastAttempt.DateUtc.Equals(record.Gemini.MetadataDate));
        }

        public static bool IsPublishedToGovAndUpToDate(this Record record)
        {
            return record.Publication?.Gov?.LastSuccess != null
                && record.Publication.Gov.LastSuccess.DateUtc.Equals(record.Gemini.MetadataDate);
        }

        public static bool IsPublishedToHubAndUpToDate(this Record record)
        {
            return record.Publication?.Hub?.LastSuccess != null
                   && record.Publication.Hub.LastSuccess.DateUtc.Equals(record.Gemini.MetadataDate);
        }

        public static bool HasPublishingDestination(this Record record)
        {
            return record.Publication != null &&
                   (record.Publication.Hub != null && record.Publication.Hub.Publishable ||
                    record.Publication.Gov != null && record.Publication.Gov.Publishable == true);
        }
    }
}
