using Catalogue.Utilities.Logging;
using Catalogue.Data.Model;
using System;
using Catalogue.Utilities.Time;
using log4net;
using Raven.Client;

namespace Catalogue.Data.Write
{
    public class OpenDataPublishingUploadService : RecordService, IOpenDataPublishingUploadService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OpenDataPublishingUploadService));

        public OpenDataPublishingUploadService(IDocumentSession db, IRecordValidator validator) : base(db, validator)
        {
        }

        public void UpdateLastAttempt(Record record, PublicationAttempt attempt)
        {
            record.Publication.OpenData.LastAttempt = attempt;
            UpdateMetadataDate(record, attempt.DateUtc);

            var userInfo = new UserInfo
            {
                DisplayName = "Robot Uploader",
                Email = "data@jncc.gov.uk"
            };
            SetFooterForUpdatedRecord(record, userInfo);

            var recordServiceResult = Upsert(record);
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

            var recordServiceResult = Upsert(record);
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

        private void SetFooterForUpdatedRecord(Record record, UserInfo userInfo)
        {
            record.Footer.ModifiedOnUtc = Clock.NowUtc;
            record.Footer.ModifiedByUser = userInfo;
        }
    }
}
