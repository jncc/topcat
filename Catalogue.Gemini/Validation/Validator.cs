using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Catalogue.Gemini.Validation
{
    public interface IValidator
    {
        /// <summary>
        /// Validates the ISO XML document using the CEH web service.
        /// </summary>
        XDocument Validate(XDocument doc);
    }

    public class Validator : IValidator
    {
        public XDocument Validate(XDocument doc)
        {
            // see http://metacheck.nerc-lancaster.ac.uk/validator

            var c = new WebClient();
            c.Headers[HttpRequestHeader.ContentType] = "application/vnd.iso.19139+xml";
            c.Headers[HttpRequestHeader.Accept] = "application/xml";
            
            var result = c.UploadString("http://metacheck.nerc-lancaster.ac.uk/validator", doc.ToString());

            return XDocument.Parse(result);
        }
    }
}
