using Catalogue.Data.Model;
using log4net;

namespace Catalogue.Robot.Publishing.Gov
{
    public class MetadataUploader : IMetadataUploader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MetadataUploader));

        private readonly Env env;
        private readonly IFtpClient ftpClient;
        private readonly IXmlHelper xmlHelper;

        public MetadataUploader(Env env)
        {
            this.env = env;
            ftpClient = new FtpClient(env.FTP_USERNAME, env.FTP_PASSWORD);
            xmlHelper = new XmlHelper();
        }

        public void UploadMetadataDocument(Record record)
        {
            var metaXmlDoc = xmlHelper.GetMetadataDocument(record);
            string metaPath = $"waf/{record.Id}.xml";
            string metaFtpPath = env.FTP_ROOT_URL + "/" + metaPath;

            Logger.Info("Metadata file path: " + metaPath);
            Logger.Info("Metadata FTP path: " + metaFtpPath);

            ftpClient.UploadBytes(metaFtpPath, metaXmlDoc);
            Logger.Info("Uploaded metadata document successfully");
        }

        public void UploadWafIndexDocument(Record record)
        {
            string indexDocFtpPath = $"{env.FTP_ROOT_URL}/waf/index.html";
            string indexDocHtml = ftpClient.DownloadString(indexDocFtpPath);
            string updatedIndexDoc = xmlHelper.UpdateWafIndexDocument(record, indexDocHtml);
            ftpClient.UploadString(indexDocFtpPath, updatedIndexDoc);
        }
    }
}
