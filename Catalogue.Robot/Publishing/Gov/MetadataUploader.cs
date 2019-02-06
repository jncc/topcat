using Catalogue.Data.Model;
using Catalogue.Data.Write;
using log4net;

namespace Catalogue.Robot.Publishing.Gov
{
    public class MetadataUploader : IMetadataUploader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MetadataUploader));

        private readonly UploaderConfig config;
        private readonly IFtpClient ftpClient;
        private readonly IXmlHelper xmlHelper;

        public MetadataUploader(UploaderConfig config)
        {
            this.config = config;
            ftpClient = new FtpClient(config.FtpUsername, config.FtpPassword);
            xmlHelper = new XmlHelper();
        }

        public void UploadMetadataDocument(Record record)
        {
            var metaXmlDoc = xmlHelper.GetMetadataDocument(record);
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
    }
}
