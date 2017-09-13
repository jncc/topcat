using Catalogue.Utilities.Logging;
using Catalogue.Data.Model;
using System;
using log4net;

namespace Catalogue.Data.Write
{
    public class OpenDataPublishingUploadService : IOpenDataPublishingUploadService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(OpenDataPublishingUploadService));

        private readonly IRecordService recordService;

        public OpenDataPublishingUploadService(IRecordService recordService)
        {
            this.recordService = recordService;
        }

        public void UpdateLastAttempt(Record record, PublicationAttempt attempt, UserInfo userInfo)
        {
            // save a not-yet-successful attempt to begin with
            record.Publication.OpenData.LastAttempt = attempt;

            var recordServiceResult = recordService.Update(record, userInfo);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(logger);
            }
        }

        public void UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(Record record)
        {
            // this is a big dataset so just link to a webpage
            string jnccWebDownloadPage = "http://jncc.defra.gov.uk/opendata";
            record.Gemini.ResourceLocator = jnccWebDownloadPage;
            logger.Info("ResourceLocator updated to point to open data request webpage.");
        }

        public void UpdateResourceLocatorToMatchMainDataFile(Record record, string dataHttpPath)
        {
            // update the resource locator to be the data file
            record.Gemini.ResourceLocator = dataHttpPath;
            logger.Info("ResourceLocator updated to point to the data file.");
        }
    }
}
