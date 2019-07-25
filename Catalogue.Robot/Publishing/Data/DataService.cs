using Catalogue.Robot.Publishing.Client;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Utilities.DriveMapping;
using Catalogue.Utilities.Text;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalogue.Robot.Publishing.Data
{
    public interface IDataService
    {
        List<string> GetDataFiles(string recordId);
        void CreateDataRollback(string recordId);
        void UploadDataFile(string recordId, string filePath);
        void RemoveRollbackFiles(string recordId);
        void Rollback(string recordId);
        void ReportRemovedDataFiles(List<string> fileUrls);
    }

    public class DataService : IDataService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DataService));

        private readonly Env env;
        private readonly IFtpClient ftpClient;
        private readonly IFileHelper fileHelper;
        private readonly ISmtpClient smtpClient;

        public DataService(Env env, IFtpClient ftpClient, IFileHelper fileHelper, ISmtpClient smtpClient)
        {
            this.env = env;
            this.ftpClient = ftpClient;
            this.fileHelper = fileHelper;
            this.smtpClient = smtpClient;
        }

        public List<string> GetDataFiles(string recordId)
        {
            var folderPath = $"{env.FTP_DATA_FOLDER}/{recordId}";

            var dataFiles = new List<string>();
            if (ftpClient.FolderExists(folderPath))
            {
                dataFiles = ftpClient.ListFolder(folderPath);
            }

            return dataFiles;
        }

        public void CreateDataRollback(string recordId)
        {
            var sourceFolder = $"{env.FTP_DATA_FOLDER}/{recordId}";
            var destinationFolder = $"{env.FTP_ROLLBACK_FOLDER}/{recordId}";
            
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

        public void RemoveRollbackFiles(string recordId)
        {
            var oldFolderPath = $"{env.FTP_ROLLBACK_FOLDER}/{recordId}";

            if (ftpClient.FolderExists(oldFolderPath))
            {
                Logger.Info($"Cleaning up old data for {recordId}");
                ftpClient.DeleteFolder(oldFolderPath);
            }
            else
            {
                Logger.Info($"No data clean up required for {recordId}");
            }
        }

        public void Rollback(string recordId)
        {
            Logger.Info($"Rolling back to use old data for {recordId}");
            var oldFolder = $"{env.FTP_ROLLBACK_FOLDER}/{recordId}";
            var dataFolder = $"{env.FTP_DATA_FOLDER}/{recordId}";

            if (ftpClient.FolderExists(dataFolder))
            {
                ftpClient.DeleteFolder(dataFolder);
            }
            ftpClient.MoveFolder(oldFolder, dataFolder);
            Logger.Info($"Rollback successful for {recordId}");
        }

        public void ReportRemovedDataFiles(List<string> fileUrls)
        {
            if (fileUrls != null && fileUrls.Count > 0)
            {
                Logger.Info($"Sending removed data files report with the following URLs: {string.Join(", ", fileUrls)}");
                var body = new StringBuilder();
                body.AppendLine($"Report for Topcat robot run {DateTime.Now}");
                body.AppendLine();
                body.AppendLine("The following files have been removed:");
                foreach (var url in fileUrls)
                {
                    body.AppendLine(url);
                }

                smtpClient.SendEmail(env.SMTP_FROM, env.SMTP_TO, "MEOW - Removed data files", body.ToString());
            }
            else
            {
                Logger.Info("No data files removed, reporting not needed");
            }
            
        }
    }
}
