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
        RecordQueryer queryer;

        public ExportController(RecordQueryer queryer)
        {
            this.queryer = queryer;
        }

        public HttpResponseMessage Get([FromUri] RecordQueryInputModel input)
        {
            // todo will need to use ravendb streaming or increase db page size for larger exports

            // ignore paging (p and n) parameters - exporting always returns the full record set
            input.P = 0;
            input.N = 1024;
            
            var results = queryer.RecordQuery(input).ToList();

            if (results.Count >= 1024)
                throw new InvalidOperationException("We don't support exports this large yet.");

            var writer = new StringWriter();
            var exporter = new Exporter();
            exporter.Export(results, writer);

            var result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent(writer.ToString())};

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "topcat-export-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt"
                };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}
