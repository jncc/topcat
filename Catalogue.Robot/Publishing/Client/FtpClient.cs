using System.Net;
using System.Text;

namespace Catalogue.Robot.Publishing.Client
{
    /// <summary>
    /// Mockable FluentFtp client.
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
            var client = new FluentFTP.FtpClient(env.FTP_HOST);
            client.Credentials = new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD);
            client.Connect();

            try
            {
                client.UploadFile(filepath, ftpPath, createRemoteDir: true);
            }
            finally
            {
                client.Disconnect();
            }
        }

        public void UploadString(string ftpPath, string content)
        {
            UploadBytes(ftpPath, Encoding.UTF8.GetBytes(content));
        }

        public void UploadBytes(string ftpPath, byte[] bytes)
        {
            var client = new FluentFTP.FtpClient(env.FTP_HOST);
            client.Credentials = new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD);
            client.Connect();

            try
            {
                client.Upload(bytes, ftpPath, createRemoteDir: true);
            }
            finally
            {
                client.Disconnect();
            }
        }

        public string DownloadString(string ftpPath)
        {
            var client = new FluentFTP.FtpClient(env.FTP_HOST);
            client.Credentials = new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD);
            client.Connect();

            try
            {
                client.Download(out byte[] outBytes, ftpPath);
                return Encoding.UTF8.GetString(outBytes);
            }
            finally
            {
                client.Disconnect();
            }
        }
    }
}
