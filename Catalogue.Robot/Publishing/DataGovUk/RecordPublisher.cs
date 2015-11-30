using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Robot.Publishing.DataGovUk
{
    public class RecordPublisher
    {
        readonly IDocumentSession db;
        readonly DataGovUkPublisherConfig config;
        readonly IFtpClient ftpClient;

        public RecordPublisher(IDocumentSession db, DataGovUkPublisherConfig config, IFtpClient ftpClient)
        {
            this.db = db;
            this.config = config;
            this.ftpClient = ftpClient;
        }

        public void PublishRecord(Guid id)
        {
            var record = db.Load<Record>(id);
            Console.WriteLine("Publishing '{0}' to '{1}'.", record.Gemini.Title, config.FtpRootUrl);

            string metaPath = String.Format("waf/{0}.xml", record.Id);
          //string dataPath = String.Format("data/{0}/{1}", record.Id, WebUtility.UrlEncode(Path.GetFileName(record.Path)));
            string dataPath = String.Format("data/{0}", WebUtility.UrlEncode(Path.GetFileName(record.Path)));

            // save a not-yet-successful attempt to begin with
            var attempt = AddNewAttempt(record);
            db.SaveChanges();

            // do the sequential actions
            UploadTheDataFile(record, dataPath);
            UpdateResourceLocatorInTheRecord(record, dataPath);
            UploadTheMetadataDocument(record, metaPath);
            UpdateTheWafIndexDocument(record);

            // mark the attempt successful
            attempt.Successful = true;

            // commit the changes (to the resource locator and the attempt object)
            db.SaveChanges();
        }

        PublicationAttempt AddNewAttempt(Record record)
        {
            var publicationInfo = record.Publication.DataGovUk;
            if (publicationInfo.Attempts == null)
                publicationInfo.Attempts = new List<PublicationAttempt>();

            var attempt = new PublicationAttempt { DateUtc = DateTime.UtcNow };
            publicationInfo.Attempts.Add(attempt);

            return attempt;
        }

        void UploadTheDataFile(Record record, string dataPath)
        {
            string dataFtpPath = config.FtpRootUrl + "/" + dataPath;
            Console.WriteLine("Uploading data file to {0}", dataFtpPath);

            if (!File.Exists(record.Path))
                throw new Exception(String.Format("Data file for record {0} doesn't exist.", record.Id));

            ftpClient.UploadFile(dataFtpPath, record.Path);
            Console.WriteLine("Uploaded data file successfully.");
        }

        void UpdateResourceLocatorInTheRecord(Record record, string dataPath)
        {
            string dataHttpPath = config.HttpRootUrl + "/" + dataPath;
            record.Gemini.ResourceLocator = dataHttpPath;
        }

        void UploadTheMetadataDocument(Record record, string metaPath)
        {
            string metaXmlDoc = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini).ToString();
            string metaFtpPath = config.FtpRootUrl + "/" + metaPath;

            ftpClient.UploadString(metaFtpPath, metaXmlDoc);
        }

        void UpdateTheWafIndexDocument(Record record)
        {
            string indexDocFtpPath = String.Format("{0}/waf/index.html", config.FtpRootUrl);
            string indexDocHtml = ftpClient.DownloadString(indexDocFtpPath);

            var doc = XDocument.Parse(indexDocHtml);
            var body = doc.Root.Element("body");
            body.Add(new XElement("a", new XAttribute("href", record.Id + ".xml")));

            ftpClient.UploadString(indexDocFtpPath, doc.ToString());
        }
    }
}
