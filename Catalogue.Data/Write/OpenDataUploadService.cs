using Catalogue.Data.Model;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Catalogue.Gemini.Model;
using Catalogue.Robot.Publishing.OpenData;
using static Catalogue.Utilities.Text.WebificationUtility;

namespace Catalogue.Data.Write
{
    public class OpenDataUploadService : IOpenDataUploadService
    {
        private readonly OpenDataPublisherConfig config;
        private readonly IFtpClient ftpClient;

        public OpenDataUploadService(OpenDataPublisherConfig config)
        {
            this.config = config;
            ftpClient = new FtpClient(config.FtpUsername, config.FtpPassword);
        }

        public void UploadDataFile(Guid recordId, string filePath, bool metadataOnly)
        {
            if (!metadataOnly) // if metadataOnly, we don't really upload the data file
            {
                // correct path for unmapped drive X
                filePath = filePath.Replace(@"X:\OffshoreSurvey\", @"\\JNCC-CORPFILE\Marine Survey\OffshoreSurvey\");
                //filePath = filePath.Replace(@"J:\GISprojects\", @"\\Jncc - corpfile\gis\GISprojects\");
        
                string unrootedDataPath = GetUnrootedDataPath(recordId, filePath);
        
                string dataFtpPath = config.FtpRootUrl + "/" + unrootedDataPath;
                Console.WriteLine("Uploading file...");
                Console.WriteLine(filePath);
                Console.WriteLine(dataFtpPath);
        
                ftpClient.UploadFile(dataFtpPath, filePath);
                Console.WriteLine("Uploaded data file successfully.");
            }
        }

        public void UploadAlternativeResources(Record record, bool metadataOnly)
        {
            // check no duplicate filenames after webifying
            var fileNames = from r in record.Publication.OpenData.Resources
                let fileName = ToUrlFriendlyString(Path.GetFileName(r.Path))
                group r by fileName;
            if (fileNames.Count() != record.Publication.OpenData.Resources.Count)
                throw new Exception("There are duplicate resource file names (after webifying) for this record.");

            // upload the resources
            foreach (var r in record.Publication.OpenData.Resources)
            {
                UploadDataFile(record.Id, r.Path, metadataOnly);
            }
        }

        public void UploadMetadataDocument(Record record)
        {
            var metaXmlDoc = GetMetadataDocument(record);
            string metaPath = String.Format("waf/{0}.xml", record.Id);
            string metaFtpPath = config.FtpRootUrl + "/" + metaPath;

            ftpClient.UploadBytes(metaFtpPath, metaXmlDoc);
        }

        public void UploadWafIndexDocument(Record record)
        {
            string indexDocFtpPath = String.Format("{0}/waf/index.html", config.FtpRootUrl);
            string indexDocHtml = ftpClient.DownloadString(indexDocFtpPath);
            string updatedIndexDoc = UpdateWafIndexDocument(record, indexDocHtml);
            ftpClient.UploadString(indexDocFtpPath, updatedIndexDoc);
        }

        public string GetHttpRootUrl()
        {
            return config.HttpRootUrl;
        }

        private byte[] GetMetadataDocument(Record record)
        {
            bool alternativeResources = record.Publication != null && record.Publication.OpenData != null && record.Publication.OpenData.Resources != null && record.Publication.OpenData.Resources.Any();
            var doc = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini);

            if (alternativeResources)
            {
                // mung (mutate) the metadata doc so data.gov.uk knows about the resources
                var onlineResources = record.Publication.OpenData.Resources
                    .Select(r => new OnlineResource
                    {
                        Name = ToUrlFriendlyString(Path.GetFileName(r.Path)),
                        Url = config.HttpRootUrl + "/" + GetUnrootedDataPath(record.Id, r.Path)
                    }).ToList();

                global::Catalogue.Gemini.Encoding.XmlEncoder.ReplaceDigitalTransferOptions(doc, onlineResources);
            }

            var s = new MemoryStream();
            doc.Save(s);

            return s.ToArray();
        }

        private string UpdateWafIndexDocument(Record record, string indexDocHtml)
        {
            var doc = XDocument.Parse(indexDocHtml);
            var body = doc.Root.Element("body");

            var newLink = new XElement("a", new XAttribute("href", record.Id + ".xml"), record.Gemini.Title);
            var existingLinks = body.Elements("a").ToList();

            existingLinks.Remove();

            var newLinks = existingLinks
                .Concat(new[] { newLink })
                .GroupBy(a => a.Attribute("href").Value)
                .Select(g => g.First()); // poor man's DistinctBy

            body.Add(newLinks);

            return doc.ToString();
        }
    }
}
