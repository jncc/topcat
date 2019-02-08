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

        public FtpClient(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public void UploadFile(string ftpPath, string filepath)
        {
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
    }
}
