using Catalogue.Robot.Publishing.Client;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Utilities.DriveMapping;
using Catalogue.Utilities.Text;
using log4net;
using System;

namespace Catalogue.Robot.Publishing.Data
{
    public interface IDataUploader
    {
        void MoveFolderIfExists(string recordId);
        void UploadDataFile(string recordId, string filePath);
        void Rollback(string recordId);
    }

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

        public void MoveFolderIfExists(string recordId)
        {
            var sourceFolder = $"{env.FTP_DATA_FOLDER}/{recordId}";
            var destinationFolder = $"{env.FTP_OLD_FOLDER}/{recordId}";
            
            ftpClient.MoveFolder(sourceFolder, destinationFolder);
        }

        public void UploadDataFile(string recordId, string filePath)
        {
            filePath = JnccDriveMappings.GetUncPath(filePath);

            var fileSize = fileHelper.GetFileSizeInBytes(filePath);

            if (fileSize <= env.MAX_FILE_SIZE_IN_BYTES)
            {
                string dataFtpPath = WebificationUtility.GetUnrootedDataPath(recordId, filePath);
                
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

        public void Rollback(string recordId)
        {
            var oldFolder = $"{env.FTP_OLD_FOLDER}/{recordId}";
            var dataFolder = $"{env.FTP_DATA_FOLDER}/{recordId}";

            ftpClient.DeleteFolder(dataFolder);
            ftpClient.MoveFolder(oldFolder, dataFolder);
        }
    }
}
