using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Catalogue.Data.Export;
using Catalogue.Data.Model;
using Catalogue.Utilities.Clone;
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
            var records = GetRecords(input);

            var writer = new StringWriter();
            new Exporter().Export(records, writer);

            var result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent(writer.ToString())};

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "topcat-export-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt"
                };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }

        /// <summary>
        /// This is public because I don't have time right now to split things up to test it any better.
        /// </summary>
        public List<Record> GetRecords(RecordQueryInputModel input)
        {
            // todo will need to use ravendb streaming or increase db page size for larger exports            

            var query = input.With(x =>
                {
                    x.P = 0;
                    x.N = 1024;
                });

            var results = queryer.RecordQuery(query).ToList();

            if (results.Count >= 1024)
                throw new InvalidOperationException("We don't support exports this large yet.");

            return results;
        }
    }
}
