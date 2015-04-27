using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Catalogue.Data.Model;
using Catalogue.Gemini.Encoding;
using Raven.Client;

namespace Catalogue.Web.Controllers.Download
{
    public class DownloadController : ApiController
    {
        readonly IDocumentSession db;

        public DownloadController(IDocumentSession db)
        {
            this.db = db;
        }

        public HttpResponseMessage Get(Guid id)
        {
            var record = db.Load<Record>(id);

            var xml = new XmlEncoder().Create(record.Id, record.Gemini);
            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(xml.ToString()) };

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "topcat-record-" + record.Id.ToString().ToLower() + ".xml"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            return result;
        }

    }
}