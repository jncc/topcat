using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Catalogue.Gemini.Validation
{
    public interface IValidator
    {
        /// <summary>
        /// Validates the ISO XML document using the CEH web service.
        /// </summary>
        ValidationResultSet Validate(XDocument doc);
    }

    public class Validator : IValidator
    {
        public ValidationResultSet Validate(XDocument doc)
        {
            // see http://metacheck.nerc-lancaster.ac.uk/validator
            System.Console.WriteLine(doc);
            string result = ExecuteCehRequest(doc);
            return ParseCehResult(result);
        }

        string ExecuteCehRequest(XDocument doc)
        {
            var c = new WebClient();
            c.Headers[HttpRequestHeader.ContentType] = "application/vnd.iso.19139+xml";
            c.Headers[HttpRequestHeader.Accept] = "application/xml";
            
            return c.UploadString("http://metacheck.nerc-lancaster.ac.uk/validator", doc.ToString());
        }

        ValidationResultSet ParseCehResult(string cehResult)
        {
            var doc = XDocument.Parse(cehResult);

            // note: this parsing was done by inspecting the XML result that is returned
            // so there's no guarantee it's exhaustive - if in doubt, debug the actual XML.
            
            // ReSharper disable PossibleNullReferenceException
            System.Console.WriteLine(doc);
            return new ValidationResultSet
                {
                    FileIdentifier = doc.Root.Element("fileIdentifier").Value,
                    Results = (from r in doc.Root.Elements("result")
                               select new ValidationResult
                               {
                                   Validation = r.Element("validation").Value,
                                   Status = r.Element("status").Value,
                                   Errors = (from e in r.Elements("error")
                                             select new ValidationError
                                                 {
                                                     Location = e.Element("location").Value,
                                                     FailedAssertion = e.Element("failedAssertion").Value,
                                                     Message = e.Element("message").Value,
                                                 }).ToList()
                               }).ToList()
                };
        }
    }
}
