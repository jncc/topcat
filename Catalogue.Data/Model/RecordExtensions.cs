using System;

namespace Catalogue.Data.Model
{
    public static class RecordExtensions
    {
        public static Boolean IsAssessedAndUpToDate(this Record record)
        {
            return record.Publication != null && record.Publication.OpenData.Assessment != null 
                && record.Publication.OpenData.Assessment.Completed
                && (record.Publication.OpenData.Assessment.CompletedOnUtc.Equals(record.Gemini.MetadataDate)
                || IsSignedOffAndUpToDate(record));
        }

        public static Boolean IsSignedOffAndUpToDate(this Record record)
        {
            return record.Publication != null && record.Publication.OpenData != null && record.Publication.OpenData.SignOff != null
                && (record.Publication.OpenData.SignOff.DateUtc.Equals(record.Gemini.MetadataDate)
                || record.Publication.OpenData.LastAttempt != null
                && record.Publication.OpenData.LastAttempt.DateUtc.Equals(record.Gemini.MetadataDate));
        }
    }
}
