using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class OpenDataXmlHelper : IOpenDataXmlHelper
    {
        public byte[] GetMetadataDocument(Record record, string resourceUrl)
        {
            bool alternativeResources = record.Publication != null && record.Publication.OpenData != null && record.Publication.OpenData.Resources != null && record.Publication.OpenData.Resources.Any();
            var doc = new Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini);

            if (alternativeResources)
            {
                // mung (mutate) the metadata doc so data.gov.uk knows about the resources
                var onlineResources = record.Publication.OpenData.Resources
                    .Select(r => new OnlineResource
                    {
                        Name = WebificationUtility.ToUrlFriendlyString(Path.GetFileName(r.Path)),
                        Url = resourceUrl
                    }).ToList();

                Gemini.Encoding.XmlEncoder.ReplaceDigitalTransferOptions(doc, onlineResources);
            }

            var s = new MemoryStream();
            doc.Save(s);

            return s.ToArray();
        }

        public string UpdateWafIndexDocument(Record record, string indexDocHtml)
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
