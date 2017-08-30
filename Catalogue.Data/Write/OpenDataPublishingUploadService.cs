using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Data.Write
{
    public class OpenDataPublishingUploadService : IOpenDataPublishingUploadService
    {
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
                throw new Exception("Error while saving upload changes.");
            }
        }

        public void UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(Record record)
        {
            // this is a big dataset so just link to a webpage
            string jnccWebDownloadPage = "http://jncc.defra.gov.uk/opendata";
            record.Gemini.ResourceLocator = jnccWebDownloadPage;
            Console.WriteLine("ResourceLocator updated to point to open data request webpage.");
        }

        public void UpdateResourceLocatorToMatchMainDataFile(Record record, string dataHttpPath)
        {
            // update the resource locator to be the data file
            record.Gemini.ResourceLocator = dataHttpPath;
            Console.WriteLine("ResourceLocator updated to point to the data file.");
        }
    }
}
