﻿using System;
using Catalogue.Data.Model;

namespace Catalogue.Data.Extensions
{
    public static class RecordExtensions
    {

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
    }
}
