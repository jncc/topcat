using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Gemini.Encoding;
using Raven.Client.Documents.Session;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Catalogue.Web.Controllers.Download
{
    public class DownloadController : ApiController
    {
        readonly IDocumentSession db;

        public DownloadController(IDocumentSession db)
        {
            this.db = db;
        }

        public HttpResponseMessage Get(string id)
        {
            var record = db.Load<Record>(Helpers.AddCollection(id));
            record = Helpers.RemoveCollectionFromId(record);
            var resources = Helpers.GetOnlineResourcesFromDataResources(record);

            var xml = new XmlEncoder().Create(record.Id, record.Gemini, resources);
            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(xml.ToString()) };

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "topcat-record-" + record.Id.ToLower() + ".xml"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            return result;
        }

    }
}