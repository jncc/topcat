using System.IO;
using System.Net;
using log4net;

namespace Catalogue.Robot.Publishing.Client
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
        private readonly Env env;

        public FtpClient(Env env)
        {
            this.env = env;
        }

        public void UploadFile(string ftpPath, string filepath)
        {
            using (var c = new WebClient { Credentials = new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD), Proxy = null })
            {
                c.UploadFile(ftpPath, "STOR", filepath);
            } 
        }

        public void UploadString(string ftpPath, string content)
        {
            using (var c = new WebClient {Credentials = new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD), Proxy = null})
            {
                c.UploadString(ftpPath, "STOR", content);
            }
        }

        public void UploadBytes(string ftpPath, byte[] bytes)
        {
            using (var c = new WebClient {Credentials = new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD), Proxy = null})
            {
                c.UploadData(ftpPath, "STOR", bytes);
            }
        }

        public string DownloadString(string ftpPath)
        {
            using (var c = new WebClient {Credentials = new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD), Proxy = null})
            {
                return c.DownloadString(ftpPath);
            }
        }
    }
}
