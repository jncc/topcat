using System;
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
                || record.Publication.Target.Hub?.LastAttempt != null
                && record.Publication.Target.Hub.LastAttempt.DateUtc.Equals(record.Gemini.MetadataDate)
                || record.Publication.Target.Gov?.LastAttempt != null
                && record.Publication.Target.Gov.LastAttempt.DateUtc.Equals(record.Gemini.MetadataDate));
        }

        public static bool IsPublishedToGovAndUpToDate(this Record record)
        {
            return record.Publication?.Target?.Gov?.LastSuccess != null
                && record.Publication.Target.Gov.LastSuccess.DateUtc.Equals(record.Gemini.MetadataDate);
        }

        public static bool IsPublishedToHubAndUpToDate(this Record record)
        {
            return record.Publication?.Target?.Hub?.LastSuccess != null
                   && record.Publication.Target.Hub.LastSuccess.DateUtc.Equals(record.Gemini.MetadataDate);
        }

        public static bool HasPublishingTarget(this Record record)
        {
            return record.Publication != null && record.Publication.Target != null &&
                   (record.Publication.Target.Hub != null && record.Publication.Target.Hub.Publishable == true ||
                   record.Publication.Target.Gov != null && record.Publication.Target.Gov.Publishable == true);
        }
    }
}
