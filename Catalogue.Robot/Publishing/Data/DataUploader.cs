using Catalogue.Utilities.DriveMapping;
using Catalogue.Utilities.Text;
using log4net;

namespace Catalogue.Robot.Publishing.Data
{
    public class DataUploader : IDataUploader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DataUploader));

        private readonly Env env;
        private readonly IFtpClient ftpClient;

        public DataUploader(Env env)
        {
            this.env = env;
            ftpClient = new FtpClient(env.FTP_USERNAME, env.FTP_PASSWORD);
        }

        public void UploadDataFile(string recordId, string filePath)
        {
            filePath = JnccDriveMappings.GetUncPath(filePath);

            string unrootedDataPath = WebificationUtility.GetUnrootedDataPath(recordId, filePath);
        
            string dataFtpPath = env.FTP_ROOT_URL + "/" + unrootedDataPath;
            Logger.Info("Data file path: "+filePath);
            Logger.Info("Data FTP path: "+dataFtpPath);
        
            ftpClient.UploadFile(dataFtpPath, filePath);
            Logger.Info("Uploaded data file successfully");
        }

        public string GetHttpRootUrl()
        {
            return env.HTTP_ROOT_URL;
        }
    }
}
