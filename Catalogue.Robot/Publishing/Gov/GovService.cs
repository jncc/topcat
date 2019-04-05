using Catalogue.Data.Model;
using Catalogue.Robot.Publishing.Client;
using log4net;

namespace Catalogue.Robot.Publishing.Gov
{
    public interface IGovService
    {
        void UploadGeminiXml(Record record);
        void UpdateDguIndex(Record record);
    }

    public class GovService : IGovService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GovService));
        
        private readonly IFtpClient ftpClient;
        private readonly IXmlHelper xmlHelper;

        public GovService(IFtpClient ftpClient)
        {
            this.ftpClient = ftpClient;
            xmlHelper = new XmlHelper();
        }

        public void UploadGeminiXml(Record record)
        {
            var metaXmlDoc = xmlHelper.GetMetadataDocument(record);
            string metaFtpPath = $"/waf/{record.Id}.xml";
            
            Logger.Info("Metadata FTP path: " + metaFtpPath);

            ftpClient.UploadBytes(metaFtpPath, metaXmlDoc);
            Logger.Info("Uploaded metadata document successfully");
        }

        public void UpdateDguIndex(Record record)
        {
            string indexDocFtpPath = $"/waf/index.html";
            string indexDocHtml = ftpClient.DownloadString(indexDocFtpPath);
            string updatedIndexDoc = xmlHelper.UpdateWafIndexDocument(record, indexDocHtml);
            ftpClient.UploadString(indexDocFtpPath, updatedIndexDoc);
        }
    }
}
