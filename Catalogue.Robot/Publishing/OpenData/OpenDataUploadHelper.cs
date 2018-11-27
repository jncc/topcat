using System;
using System.IO;
using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Utilities.DriveMapping;
using Catalogue.Utilities.Logging;
using Catalogue.Utilities.Text;
using log4net;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class OpenDataUploadHelper : IOpenDataUploadHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OpenDataUploadHelper));

        private readonly OpenDataUploadConfig config;
        private readonly IFtpClient ftpClient;
        private readonly IOpenDataXmlHelper xmlHelper;

        public OpenDataUploadHelper(OpenDataUploadConfig config)
        {
            this.config = config;
            ftpClient = new FtpClient(config.FtpUsername, config.FtpPassword);
            xmlHelper = new OpenDataXmlHelper();
        }

        public void UploadDataFile(string recordId, string filePath)
        {
            filePath = JnccDriveMappings.GetUncPath(filePath);

            string unrootedDataPath = WebificationUtility.GetUnrootedDataPath(recordId, filePath);
        
            string dataFtpPath = config.FtpRootUrl + "/" + unrootedDataPath;
            Logger.Info("Data file path: "+filePath);
            Logger.Info("Data FTP path: "+dataFtpPath);
        
            ftpClient.UploadFile(dataFtpPath, filePath);
            Logger.Info("Uploaded data file successfully");
        }

        public void UploadMetadataDocument(Record record)
        {
            string resourceUrl = config.HttpRootUrl + "/" + WebificationUtility.GetUnrootedDataPath(record.Id, record.Path);
            var metaXmlDoc = xmlHelper.GetMetadataDocument(record, resourceUrl);
            string metaPath = $"waf/{record.Id}.xml";
            string metaFtpPath = config.FtpRootUrl + "/" + metaPath;

            Logger.Info("Metadata file path: " + metaPath);
            Logger.Info("Metadata FTP path: " + metaFtpPath);

            ftpClient.UploadBytes(metaFtpPath, metaXmlDoc);
            Logger.Info("Uploaded metadata document successfully");
        }

        public void UploadWafIndexDocument(Record record)
        {
            string indexDocFtpPath = $"{config.FtpRootUrl}/waf/index.html";
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
