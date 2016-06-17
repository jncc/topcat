using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using Catalogue.Utilities.Time;
using Raven.Client;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class OpenDataRecordPublisher
    {
        readonly IDocumentSession db;
        readonly OpenDataPublisherConfig config;
        readonly IFtpClient ftpClient;

        public OpenDataRecordPublisher(IDocumentSession db, OpenDataPublisherConfig config, IFtpClient ftpClient)
        {
            this.db = db;
            this.config = config;
            this.ftpClient = ftpClient;
        }

        public void PublishRecord(Guid id)
        {
            var record = db.Load<Record>(id);
            Console.WriteLine(Environment.NewLine + "Publishing '{0}' to '{1}'.", record.Gemini.Title, config.FtpRootUrl);

            string metaPath = String.Format("waf/{0}.xml", record.Id);

            // save a not-yet-successful attempt to begin with
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            record.Publication.OpenData.LastAttempt = attempt;
            db.SaveChanges();

            bool datasetIsCorpulent = record.Gemini.Keywords.Any(k => k.Vocab == "http://vocab.jncc.gov.uk/metadata-admin" && k.Value == "Corpulent");
            bool publishAlternativeResources = record.Publication != null && record.Publication.OpenData != null && record.Publication.OpenData.Resources != null && record.Publication.OpenData.Resources.Any();

            var doc = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini);

            try
            {
                if (publishAlternativeResources)
                {
                    // upload the alternative resources and correct (mutate) the doc; don't touch the resource locator
                    UploadAlternativeResourcesAndMungThemIntoMetadataDoc(record, doc);
                }
                else
                {
                    if (datasetIsCorpulent)
                    {
                        // set the resource locator if blank; don't upload any resources
                        if (record.Gemini.ResourceLocator.IsBlank()) // arguably should always do it actually
                            UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(record);
                    }
                    else
                    {
                        // "normal" case - set the resource locator; upload the resource pointed at by record.Path
                        UploadNormalDataFileAndUpdateResourceLocatorToMatch(record);
                    }
                }

                UploadTheMetadataDocument(doc, metaPath);
                UpdateTheWafIndexDocument(record);

                // record success
                record.Publication.OpenData.LastSuccess = attempt;

            }
            catch (WebException ex)
            {
                string message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                Console.WriteLine(message);
                attempt.Message = message;
            }

            // commit the changes - to both the record (resource locator may have changed) and the attempt object
            db.SaveChanges();
        }

        void UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(Record record)
        {
            // this is a big dataset so just link to a webpage
            string jnccWebDownloadPage = "http://jncc.defra.gov.uk/opendata";
            record.Gemini.ResourceLocator = jnccWebDownloadPage;
            Console.WriteLine("ResourceLocator updated to point to open data request webpage.");

        }

        void UploadNormalDataFileAndUpdateResourceLocatorToMatch(Record record)
        {
            UploadFile(record.Id, record.Path);

            // update the resource locator to be the data file
            string dataHttpPath = config.HttpRootUrl + "/" + GetUnrootedDataPath(record.Id, record.Path);
            record.Gemini.ResourceLocator = dataHttpPath;
            Console.WriteLine("ResourceLocator updated to point to the data file.");
        }

        void UploadAlternativeResourcesAndMungThemIntoMetadataDoc(Record record, XDocument doc)
        {
            var resources = (from r in record.Publication.OpenData.Resources
                             let unrootedDataPath = GetUnrootedDataPath(record.Id, r.Path)
                             select new
                             {
                                 r.Path,
                                 Name = Path.GetFileName(r.Path),
                                 UnrootedDataPath = unrootedDataPath,
                                 Url =  config.HttpRootUrl + "/" + unrootedDataPath
                             })
                             .ToList();

            // check that there are no duplicate filenames after webifying for the data path
            if (resources.Count != (from r in resources group r by r.Name).Count())
                throw new Exception("There are duplicate resource file names (after webifying) for this record.");

            // upload the resources
            foreach (var r in resources)
            {
                UploadFile(record.Id, r.Path);
            }

            // mung (mutate) the metadata doc so data.gov.uk knows about the resources
            var onlineResources = resources.Select(r => new OnlineResource { Name = r.Name, Url = r.Url }).ToList();
            global::Catalogue.Gemini.Encoding.XmlEncoder.ReplaceDigitalTransferOptions(doc, onlineResources);
        }

        void UploadFile(Guid recordId, string filePath)
        {
            // correct path for unmapped drive X
            filePath = filePath.Replace(@"X:\OffshoreSurvey\", @"\\JNCC-CORPFILE\Marine Survey\OffshoreSurvey\");
            filePath = filePath.Replace(@"J:\GISprojects\", @"\\Jncc - corpfile\gis\GISprojects\");

            string unrootedDataPath = GetUnrootedDataPath(recordId, filePath);

            string dataFtpPath = config.FtpRootUrl + "/" + unrootedDataPath;
            Console.WriteLine("Uploading file at {0}", filePath);
            Console.WriteLine("Uploading data to {0}", dataFtpPath);

            ftpClient.UploadFile(dataFtpPath, filePath);
            Console.WriteLine("Uploaded data file successfully.");
        }

        void UploadTheMetadataDocument(XDocument doc, string metaPath)
        {
            var s = new MemoryStream();
            doc.Save(s);
            var metaXmlDoc = s.ToArray();

            string metaFtpPath = config.FtpRootUrl + "/" + metaPath;

            ftpClient.UploadBytes(metaFtpPath, metaXmlDoc);
        }

        void UpdateTheWafIndexDocument(Record record)
        {
            string indexDocFtpPath = String.Format("{0}/waf/index.html", config.FtpRootUrl);
            string indexDocHtml = ftpClient.DownloadString(indexDocFtpPath);

            var doc = XDocument.Parse(indexDocHtml);
            var body = doc.Root.Element("body");

            var newLink = new XElement("a", new XAttribute("href", record.Id + ".xml"), record.Gemini.Title);
            var existingLinks = body.Elements("a").ToList();
            
            existingLinks.Remove();

            var newLinks = existingLinks
                .Concat(new [] { newLink })
                .GroupBy(a => a.Attribute("href").Value)
                .Select(g => g.First()); // poor man's DistinctBy

            body.Add(newLinks);

            ftpClient.UploadString(indexDocFtpPath, doc.ToString());
        }

        /// <summary>
        /// Gets a path like "data/3148e1e2-bd6b-4623-b72a-5408263b9056-Some-Data-File.csv"
        /// </summary>
        string GetUnrootedDataPath(Guid recordId, string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            // make a file name suitable for a file on the web
            // todo: make this much more robust and web-friendly
            string name = fileName.Replace(" ", "-");

            return String.Format("data/{0}-{1}", recordId, fileName);
        }
    }
}
