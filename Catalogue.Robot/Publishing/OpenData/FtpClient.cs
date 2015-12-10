using System.Net;

namespace Catalogue.Robot.Publishing.OpenData
{
    /// <summary>
    /// Mockable FTP-focussed WebClient.
    /// </summary>
    public interface IFtpClient
    {
        void UploadFile(string address, string filename);
        void UploadString(string address, string content);
        string DownloadString(string address);
        void MakeDirectory(string address);
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
            var c = new WebClient { Credentials = new NetworkCredential(username, password) };
            c.UploadFile(address, "STOR", filename);
        }

        public void UploadString(string address, string content)
        {
            var c = new WebClient { Credentials = new NetworkCredential(username, password) };
            c.UploadString(address, "STOR", content);
        }

        public string DownloadString(string address)
        {
            var c = new WebClient { Credentials = new NetworkCredential(username, password) };
            return  c.DownloadString(address);
        }

        public void MakeDirectory(string address)
        {
            var request = WebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(username, password);
            using (request.GetResponse()) { }
        }
    }
}
