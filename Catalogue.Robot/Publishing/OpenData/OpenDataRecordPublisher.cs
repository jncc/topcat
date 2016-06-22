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
        readonly bool metadataOnly;

        public OpenDataRecordPublisher(IDocumentSession db, OpenDataPublisherConfig config, bool metadataOnly, IFtpClient ftpClient)
        {
            this.db = db;
            this.config = config;
            this.ftpClient = ftpClient;
            this.metadataOnly = metadataOnly;
        }

        public void PublishRecord(Guid id)
        {
            var record = db.Load<Record>(id);
            Console.WriteLine(Environment.NewLine + "Publishing '{0}' to '{1}'.", record.Gemini.Title, config.FtpRootUrl);

            // save a not-yet-successful attempt to begin with
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            record.Publication.OpenData.LastAttempt = attempt;

            db.SaveChanges();

            bool alternativeResources = record.Publication != null && record.Publication.OpenData != null && record.Publication.OpenData.Resources != null && record.Publication.OpenData.Resources.Any();
            bool corpulent = record.Gemini.Keywords.Any(k => k.Vocab == "http://vocab.jncc.gov.uk/metadata-admin" && k.Value == "Corpulent");

            try
            {
                if (alternativeResources)
                {
                    // upload the alternative resources; don't touch the resource locator
                    UploadAlternativeResources(record);
                }
                else
                {
                    if (corpulent)
                    {
                        // set the resource locator to the download request page; don't upload any resources
                        if (record.Gemini.ResourceLocator.IsBlank()) // arguably should always do it actually
                            UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(record);
                    }
                    else if (record.Gemini.ResourceLocator.IsBlank() || record.Gemini.ResourceLocator.Contains(config.HttpRootUrl))
                    {
                        // "normal" case - if the resource locator is blank or already data.jncc.gov.uk
                        // upload the resource pointed at by record.Path, and update the resource locator to match
                        UploadDataFile(record.Id, record.Path);
                        UpdateResourceLocatorToMatchMainDataFile(record);
                    }
                    else
                    {
                        // do nothing - don't change the resource locator, don't upload anything
                        Console.WriteLine("Deferring to existing resource locator - not uploading.");
                    }
                }

                UploadTheMetadataDocument(record, alternativeResources);
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

        void UpdateResourceLocatorToMatchMainDataFile(Record record)
        {
            // update the resource locator to be the data file
            string dataHttpPath = config.HttpRootUrl + "/" + GetUnrootedDataPath(record.Id, record.Path);
            record.Gemini.ResourceLocator = dataHttpPath;
            Console.WriteLine("ResourceLocator updated to point to the data file.");
        }

        void UploadAlternativeResources(Record record)
        {
            // check no duplicate filenames after webifying
            var fileNames = from r in record.Publication.OpenData.Resources
                            let fileName = WebificationUtility.ToUrlFriendlyString(Path.GetFileName(r.Path))
                            group r by fileName;
            if (fileNames.Count() != record.Publication.OpenData.Resources.Count)
                throw new Exception("There are duplicate resource file names (after webifying) for this record.");

            // upload the resources
            foreach (var r in record.Publication.OpenData.Resources)
            {
                UploadDataFile(record.Id, r.Path);
            }
        }

        void UploadDataFile(Guid recordId, string filePath)
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

        void UploadTheMetadataDocument(Record record,  bool alternativeResources)
        {
            var doc = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini);

            if (alternativeResources)
            {
                // mung (mutate) the metadata doc so data.gov.uk knows about the resources
                var onlineResources = record.Publication.OpenData.Resources
                    .Select(r => new OnlineResource
                    {
                        Name = WebificationUtility.ToUrlFriendlyString(Path.GetFileName(r.Path)),
                        Url = config.HttpRootUrl + "/" + GetUnrootedDataPath(record.Id, r.Path)
                    }).ToList();

                global::Catalogue.Gemini.Encoding.XmlEncoder.ReplaceDigitalTransferOptions(doc, onlineResources);
            }

            var s = new MemoryStream();
            doc.Save(s);
            var metaXmlDoc = s.ToArray();

            string metaPath = String.Format("waf/{0}.xml", record.Id);
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

            // make a file name suitable for the web
            string name = WebificationUtility.ToUrlFriendlyString(fileName);

            return String.Format("data/{0}-{1}", recordId, name);
        }
    }
}
