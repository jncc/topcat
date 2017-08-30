using Catalogue.Data.Model;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Text;
using System;
using System.IO;
using System.Linq;
using static Catalogue.Utilities.Text.WebificationUtility;

namespace Catalogue.Data.Write
{
    public class OpenDataUploadHelper : IOpenDataUploadHelper
    {
        private readonly OpenDataPublisherConfig config;
        private readonly IFtpClient ftpClient;
        private readonly IOpenDataXmlHelper xmlHelper;

        public OpenDataUploadHelper(OpenDataPublisherConfig config)
        {
            this.config = config;
            ftpClient = new FtpClient(config.FtpUsername, config.FtpPassword);
            xmlHelper = new OpenDataXmlHelper();
        }

        public void UploadDataFile(Guid recordId, string filePath, bool metadataOnly)
        {
            if (!metadataOnly) // if metadataOnly, we don't really upload the data file
            {
                // correct path for unmapped drive X
                filePath = filePath.Replace(@"X:\OffshoreSurvey\", @"\\JNCC-CORPFILE\Marine Survey\OffshoreSurvey\");
        
                string unrootedDataPath = GetUnrootedDataPath(recordId, filePath);
        
                string dataFtpPath = config.FtpRootUrl + "/" + unrootedDataPath;
                Console.WriteLine("Uploading file...");
                Console.WriteLine(filePath);
                Console.WriteLine(dataFtpPath);
        
                ftpClient.UploadFile(dataFtpPath, filePath);
                Console.WriteLine("Uploaded data file successfully.");
            }
        }

        public void UploadAlternativeResources(Record record, bool metadataOnly)
        {
            // check no duplicate filenames after webifying
            var fileNames = from r in record.Publication.OpenData.Resources
                let fileName = ToUrlFriendlyString(Path.GetFileName(r.Path))
                group r by fileName;
            if (fileNames.Count() != record.Publication.OpenData.Resources.Count)
                throw new Exception("There are duplicate resource file names (after webifying) for this record.");

            // upload the resources
            foreach (var r in record.Publication.OpenData.Resources)
            {
                UploadDataFile(record.Id, r.Path, metadataOnly);
            }
        }

        public void UploadMetadataDocument(Record record)
        {
            string resourceUrl = config.HttpRootUrl + "/" + GetUnrootedDataPath(record.Id, record.Path);
            var metaXmlDoc = xmlHelper.GetMetadataDocument(record, resourceUrl);
            string metaPath = String.Format("waf/{0}.xml", record.Id);
            string metaFtpPath = config.FtpRootUrl + "/" + metaPath;

            ftpClient.UploadBytes(metaFtpPath, metaXmlDoc);
        }

        public void UploadWafIndexDocument(Record record)
        {
            string indexDocFtpPath = String.Format("{0}/waf/index.html", config.FtpRootUrl);
            string indexDocHtml = ftpClient.DownloadString(indexDocFtpPath);
            string updatedIndexDoc = xmlHelper.UpdateWafIndexDocument(record, indexDocHtml);
            ftpClient.UploadString(indexDocFtpPath, updatedIndexDoc);
        }

        public string GetHttpRootUrl()
        {
            return config.HttpRootUrl;
        }
    }
}
