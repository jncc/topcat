using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Quartz.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Catalogue.Robot.Publishing.Gov
{
    public interface IXmlHelper
    {
        byte[] GetMetadataDocument(Record record);
        string UpdateWafIndexDocument(Record record, string indexDocHtml);
    }

    public class XmlHelper : IXmlHelper
    {
        public byte[] GetMetadataDocument(Record record)
        {
            // use either the resourcehub url or the direct data links
            var onlineResources = GetOnlineResources(record);

            // generate the XML
            var doc = new Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini, onlineResources);

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

        public static List<OnlineResource> GetOnlineResources(Record record)
        {
            var onlineResources = new List<OnlineResource>();
            if (record.Publication.Target.Hub != null && record.Publication.Target.Hub.Publishable == true &&
                record.Publication.Target.Hub.Url.IsNullOrWhiteSpace() != true)
            {
                onlineResources.Add(new OnlineResource
                {
                    Name = "JNCC ResourceHub Page",
                    Url = record.Publication.Target.Hub.Url
                });
            }
            else
            {
                onlineResources = Helpers.GetOnlineResourcesFromDataResources(record);
            }

            return onlineResources;
        }
    }
}
