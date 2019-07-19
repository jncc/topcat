using System.Net;
using System.Text;

namespace Catalogue.Robot.Publishing.Client
{
    /// <summary>
    /// Mockable FluentFtp client.
    /// </summary>
    public interface IFtpClient
    {
        void MoveFolder(string folderPath, string destinationFolderPath);
        void UploadFile(string ftpPath, string filepath);
        void UploadString(string ftpPath, string content);
        void UploadBytes(string ftpPath, byte[] bytes);
        string DownloadString(string ftpPath);
        void DeleteFolder(string folderPath);
    }

    public class FtpClient : IFtpClient
    {
        private readonly Env env;

        public FtpClient(Env env)
        {
            this.env = env;
        }

        public void MoveFolder(string folderPath, string destinationFolderPath)
        {
            using (var client =
                new FluentFTP.FtpClient(env.FTP_HOST, new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD)))
            {
                client.MoveDirectory(folderPath, destinationFolderPath);
            }
        }

        public void UploadFile(string ftpPath, string filepath)
        {
            using (var client = new FluentFTP.FtpClient(env.FTP_HOST, new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD)))
            {
                client.UploadFile(filepath, ftpPath, createRemoteDir: true);
            }
        }

        public void UploadString(string ftpPath, string content)
        {
            UploadBytes(ftpPath, Encoding.UTF8.GetBytes(content));
        }

        public void UploadBytes(string ftpPath, byte[] bytes)
        {
            using (var client = new FluentFTP.FtpClient(env.FTP_HOST, new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD)))
            {
                client.Upload(bytes, ftpPath, createRemoteDir: true);
            }
        }

        public string DownloadString(string ftpPath)
        {
            using (var client = new FluentFTP.FtpClient(env.FTP_HOST, new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD)))
            {
                client.Download(out byte[] outBytes, ftpPath);

                return Encoding.UTF8.GetString(outBytes);
            }
        }

        public void DeleteFolder(string folderPath)
        {
            using (var client = new FluentFTP.FtpClient(env.FTP_HOST, new NetworkCredential(env.FTP_USERNAME, env.FTP_PASSWORD)))
            {
                client.DeleteDirectory(folderPath);
            }
        }
    }
}
