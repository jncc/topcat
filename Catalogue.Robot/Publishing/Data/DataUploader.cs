using Catalogue.Utilities.DriveMapping;
using Catalogue.Utilities.Text;
using log4net;

namespace Catalogue.Robot.Publishing.Data
{
    public class DataUploader : IDataUploader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DataUploader));

        private readonly UploaderConfig config;
        private readonly IFtpClient ftpClient;

        public DataUploader(UploaderConfig config)
        {
            this.config = config;
            ftpClient = new FtpClient(config.FtpUsername, config.FtpPassword);
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

        public string GetHttpRootUrl()
        {
            return config.HttpRootUrl;
        }
    }
}
