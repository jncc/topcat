using Catalogue.Utilities.Logging;
using Catalogue.Data.Model;
using System;
using Catalogue.Utilities.Time;
using log4net;
using Raven.Client;
using static Catalogue.Data.Write.RecordServiceHelper;

namespace Catalogue.Data.Write
{
    public class OpenDataPublishingUploadRecordService : IOpenDataPublishingUploadRecordService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OpenDataPublishingUploadRecordService));

        private readonly IDocumentSession db;
        private readonly IRecordValidator validator;

        public OpenDataPublishingUploadRecordService(IDocumentSession db, IRecordValidator validator)
        {
            this.db = db;
            this.validator = validator;
        }

        public void UpdateLastAttempt(Record record, PublicationAttempt attempt)
        {
            record.Publication.OpenData.LastAttempt = attempt;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }

        public void UpdateLastSuccess(Record record, PublicationAttempt attempt)
        {
            record.Publication.OpenData.LastSuccess = attempt;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }

        public void UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(Record record)
        {
            // this is a big dataset so just link to a webpage
            string jnccWebDownloadPage = "http://jncc.defra.gov.uk/opendata";
            record.Gemini.ResourceLocator = jnccWebDownloadPage;
            Logger.Info("ResourceLocator updated to point to open data request webpage.");
        }

        public void UpdateResourceLocatorToMatchMainDataFile(Record record, string dataHttpPath)
        {
            // update the resource locator to be the data file
            record.Gemini.ResourceLocator = dataHttpPath;
            Logger.Info("ResourceLocator updated to point to the data file.");
        }

        private void UpdateMetadataDate(Record record, DateTime metadataDate)
        {
            record.Gemini.MetadataDate = metadataDate;
        }
    }
}
