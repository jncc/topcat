using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Catalogue.Data.Export;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Utilities.Clone;
using Newtonsoft.Json;
using Raven.Client;

namespace Catalogue.Web.Controllers.Export
{
    public class ExportController : ApiController
    {
        IDocumentSession _db;
        IRecordQueryer recordQueryer;

        public ExportController(IDocumentSession db, IRecordQueryer recordQueryer)
        {
            this._db = db;
            this.recordQueryer = recordQueryer;
        }

        /// <summary>
        /// Exports a csv file of records using the standard export format. 
        /// Ignores the paging parameter P and size parameter N
        /// </summary>
        public HttpResponseMessage Get([FromUri] RecordQueryInputModel input)
        {
            input.P = 0;
            input.N = -1; 
            using (var adb = _db.Advanced.DocumentStore.OpenAsyncSession())
            {
                var response = new HttpResponseMessage();

                response.Content = new PushStreamContent(
                    async (stream,  content,  context) =>
                    {
                        using (stream)
                        using (var enumerator = await adb.Advanced.StreamAsync(recordQueryer.AsyncRecordQuery(adb,input)))
                        {
                            var writeHeaders = true;
                            while (await enumerator.MoveNextAsync())
                            {
                                var writer = new StringWriter();
                                var exporter = new Exporter();
                                if (writeHeaders)
                                {
                                    exporter.ExportHeader(writer);
                                    writeHeaders = false;
                                }

                                exporter.ExportRecord(enumerator.Current.Document, writer);
                                var data = UTF8Encoding.UTF8.GetBytes(writer.ToString());

                                await stream.WriteAsync(data, 0, data.Length);
                            }
                        }
                    });

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "topcat-export-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".csv"
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                return response;
            }
        }
    }
}
