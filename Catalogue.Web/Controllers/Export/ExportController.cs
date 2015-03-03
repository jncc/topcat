using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Catalogue.Data.Export;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.Export
{
    public class ExportController : ApiController
    {
        IDocumentSession db;

        public ExportController(IDocumentSession db)
        {
            this.db = db;
        }

        public HttpResponseMessage Get(QueryModel input)
        {
            var keyword = ParameterHelper.ParseKeywords(new[] { input.K }).Single(); // for now, support only one

            // todo use the same query / index as search page (in SearchHelper)

            var q = from r in db.Query<Record>()
                    where r.Gemini.Keywords.Any(x => x.Vocab == keyword.Vocab && x.Value == keyword.Value)
                    select r;

            var writer = new StringWriter();
            var exporter = new Exporter();
            exporter.Export(q.ToList(), writer);

            var result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent(writer.ToString())};

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "topcat-export-" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt"
                };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}
