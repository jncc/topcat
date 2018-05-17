using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Catalogue.Utilities.Clone;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class OpenDataXmlHelper : IOpenDataXmlHelper
    {
        public byte[] GetMetadataDocument(Record record, string resourceUrl)
        {
            // redact the record for open data publishing (for GDPR etc.)
            var redacted = GetRedactedRecordToPublish(record);

            // generate the XML
            var doc = new Gemini.Encoding.XmlEncoder().Create(redacted.Id, redacted.Gemini);

            var s = new MemoryStream();
            doc.Save(s);

            return s.ToArray();
        }

        static Record GetRedactedRecordToPublish(Record record)
        {
            // create a *clone* of the record with redacted properties
            // (because we don't want to accidentally save this back to the database)
            var redacted = record.With(r =>
            {
                r.Gemini.ResponsibleOrganisation.Name = "Digital and Data Solutions, JNCC";
                r.Gemini.ResponsibleOrganisation.Email = "data@jncc.gov.uk";
                r.Gemini.MetadataPointOfContact.Name = "Digital and Data Solutions, JNCC";
                r.Gemini.MetadataPointOfContact.Email = "data@jncc.gov.uk";
            });

            return redacted;
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
