using System;
using Catalogue.Robot.Publishing.Client;
using Catalogue.Robot.Publishing.Hub;
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
        private readonly IFileHelper fileHelper;

        public DataUploader(Env env, IFtpClient ftpClient, IFileHelper fileHelper)
        {
            this.env = env;
            this.ftpClient = ftpClient;
            this.fileHelper = fileHelper;
        }

        public void UploadDataFile(string recordId, string filePath)
        {
            filePath = JnccDriveMappings.GetUncPath(filePath);

            var fileSize = fileHelper.GetFileSizeInBytes(filePath);

            if (fileSize <= env.MAX_FILE_SIZE_IN_BYTES)
            {
                string unrootedDataPath = WebificationUtility.GetUnrootedDataPath(recordId, filePath);

                string dataFtpPath = env.FTP_ROOT_URL + "/" + unrootedDataPath;
                Logger.Info("Data file path: " + filePath);
                Logger.Info("Data FTP path: " + dataFtpPath);

                ftpClient.UploadFile(dataFtpPath, filePath);
                Logger.Info("Uploaded data file successfully");
            }
            else
            {
                // force fail large files
                throw new InvalidOperationException($"File at path {filePath} is too large to be uploaded by Topcat - manual upload required");
            }
        }
    }
}
