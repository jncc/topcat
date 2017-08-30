using System.Net;

namespace Catalogue.Data.Write
{
    /// <summary>
    /// Mockable FTP-focussed WebClient.
    /// </summary>
    public interface IFtpClient
    {
        void UploadFile(string address, string filename);
        void UploadString(string address, string content);
        void UploadBytes(string address, byte[] bytes);
        string DownloadString(string address);
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

        public void UploadFile(string address, string filename)
        {
            var c = new WebClient { Credentials = new NetworkCredential(username, password), Proxy = null };
            c.UploadFile(address, "STOR", filename);
        }

        public void UploadString(string address, string content)
        {
            var c = new WebClient { Credentials = new NetworkCredential(username, password), Proxy = null };
            c.UploadString(address, "STOR", content);
        }

        public void UploadBytes(string address, byte[] bytes)
        {
            var c = new WebClient { Credentials = new NetworkCredential(username, password), Proxy = null };
            c.UploadData(address, "STOR", bytes);
        }

        public string DownloadString(string address)
        {
            var c = new WebClient { Credentials = new NetworkCredential(username, password), Proxy = null };
            return  c.DownloadString(address);
        }
    }
}
