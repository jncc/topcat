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
            return record.Publication?.Gov.Assessment != null
                && record.Publication.Gov.Assessment.Completed
                && (record.Publication.Gov.Assessment.CompletedOnUtc.Equals(record.Gemini.MetadataDate)
                || IsSignedOffAndUpToDate(record));
        }

        public static bool IsSignedOffAndUpToDate(this Record record)
        {
            return record.Publication?.Gov?.SignOff != null
                && (record.Publication.Gov.SignOff.DateUtc.Equals(record.Gemini.MetadataDate)
                || record.Publication.Gov.LastAttempt != null
                && record.Publication.Gov.LastAttempt.DateUtc.Equals(record.Gemini.MetadataDate));
        }

        public static bool IsUploadedAndUpToDate(this Record record)
        {
            return record.Publication?.Gov?.LastSuccess != null
                && record.Publication.Gov.LastSuccess.DateUtc.Equals(record.Gemini.MetadataDate);
        }
    }
}
