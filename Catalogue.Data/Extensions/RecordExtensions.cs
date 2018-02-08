using System;
using Catalogue.Data.Model;

namespace Catalogue.Data.Extensions
{
    public static class RecordExtensions
    {
        public static bool IsEligibleForOpenDataPublishing(this Record record)
        {
            var eligible = false;

            Uri uri;

            if (Uri.TryCreate(record.Path, UriKind.Absolute, out uri))
            {
                eligible = uri.IsFile;
            }

            return eligible;
        }

        public static bool IsAssessedAndUpToDate(this Record record)
        {
            return record.Publication?.OpenData.Assessment != null
                && record.Publication.OpenData.Assessment.Completed
                && (record.Publication.OpenData.Assessment.CompletedOnUtc.Equals(record.Gemini.MetadataDate)
                || IsSignedOffAndUpToDate(record));
        }

        public static bool IsSignedOffAndUpToDate(this Record record)
        {
            return record.Publication?.OpenData?.SignOff != null
                && (record.Publication.OpenData.SignOff.DateUtc.Equals(record.Gemini.MetadataDate)
                || record.Publication.OpenData.LastAttempt != null
                && record.Publication.OpenData.LastAttempt.DateUtc.Equals(record.Gemini.MetadataDate));
        }

        public static bool IsUploadedAndUpToDate(this Record record)
        {
            return record.Publication?.OpenData?.LastSuccess != null
                && record.Publication.OpenData.LastSuccess.DateUtc.Equals(record.Gemini.MetadataDate);
        }
    }
}
