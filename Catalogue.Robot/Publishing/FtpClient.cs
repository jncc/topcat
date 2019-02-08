using System.IO;
using System.Net;
using log4net;

namespace Catalogue.Robot.Publishing
{
    /// <summary>
    /// Mockable FTP-focussed WebClient.
    /// </summary>
    public interface IFtpClient
    {
        void UploadFile(string ftpPath, string filepath);
        void UploadString(string ftpPath, string content);
        void UploadBytes(string ftpPath, byte[] bytes);
        string DownloadString(string ftpPath);
    }

    public class FtpClient : IFtpClient
    {
        readonly string username;
        readonly string password;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(FtpClient));

        public FtpClient(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public void UploadFile(string ftpPath, string filepath)
        {
            var folderPath = ftpPath.Replace(Path.GetFileName(ftpPath), "");
            Logger.Info($"Folder path: {folderPath}");
            CreateFolder(folderPath);

            using (var c = new WebClient { Credentials = new NetworkCredential(username, password), Proxy = null })
            {
                c.UploadFile(ftpPath, "STOR", filepath);
            } 
        }

        public void UploadString(string ftpPath, string content)
        {
            using (var c = new WebClient {Credentials = new NetworkCredential(username, password), Proxy = null})
            {
                c.UploadString(ftpPath, "STOR", content);
            }
        }

        public void UploadBytes(string ftpPath, byte[] bytes)
        {
            using (var c = new WebClient {Credentials = new NetworkCredential(username, password), Proxy = null})
            {
                c.UploadData(ftpPath, "STOR", bytes);
            }
        }

        public string DownloadString(string ftpPath)
        {
            using (var c = new WebClient {Credentials = new NetworkCredential(username, password), Proxy = null})
            {
                return c.DownloadString(ftpPath);
            }
        }

        private void CreateFolder(string folderPath)
        {
            var credentials = new NetworkCredential(username, password);
            WebRequest request = WebRequest.Create(folderPath);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = credentials;

            // awkward handling of ftp folder creation
            try
            {
                using (request.GetResponse())
                {
                    Logger.Info($"Created directory to store data files: {folderPath}");
                }
            }
            catch (WebException we) when (we.Response is FtpWebResponse ftpWebResponse && ftpWebResponse.StatusDescription.Contains("File unavailable"))
            {
                // folder already exists, do nothing
                Logger.Info($"Record directory already exists: {folderPath}");
            }
        }
    }
}
