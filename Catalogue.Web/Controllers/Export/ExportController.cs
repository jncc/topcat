using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Xml.Linq;
using Catalogue.Data;
using Catalogue.Data.Export;
using Catalogue.Data.Query;
using Catalogue.Gemini.Encoding;

namespace Catalogue.Web.Controllers.Export
{
    public class ExportController : ApiController
    {
        IRecordQueryer recordQueryer;

        public ExportController(IRecordQueryer recordQueryer)
        {
            this.recordQueryer = recordQueryer;
        }

        void RemovePagingParametersFromRecordQuery(RecordQueryInputModel input)
        {
            input.P = 0;
            input.N = -1; 
        }

        /// <summary>
        /// Exports a csv file of records using the standard export format. 
        /// Ignores the paging parameter P and size parameter N
        /// </summary>
        public HttpResponseMessage Get([FromUri] RecordQueryInputModel input, string format)
        {
            var response = new HttpResponseMessage();
            string exportFormat;

            if (!string.IsNullOrWhiteSpace(format) && format.ToLower().Equals("csv"))
            {
                exportFormat = "csv";
                response.Content = GetResponseContent(input, ",");
            }
            else
            {
                exportFormat = "tsv";
                response.Content = GetResponseContent(input, "\t");
            }

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "topcat-export-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "." + exportFormat
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return response;
        }

        /// <summary>
        /// Exports an ISO XML file of records. 
        /// TEMPORARY. May clip the output at some number, since this is not using Raven streaming.
        /// </summary>
        [HttpGet, Route("api/export/xml")]
        public HttpResponseMessage Xml([FromUri] RecordQueryInputModel input)
        {
            RemovePagingParametersFromRecordQuery(input);

            var records = recordQueryer.Query(input).ToList();

            // encode the records as iso xml elements
            var elements = from record in records
                           let doc = new XmlEncoder().Create(record.Id, record.Gemini)
                           select new XElement("topcat-record", new XAttribute("id", record.Id), new XAttribute("path", record.Path), doc.Root);

            var output = new XDocument(new XElement("topcat-export", elements)).ToString();

            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(output) };
            return result;
        }

        private PushStreamContent GetResponseContent(RecordQueryInputModel input, string delimiter)
        {
            RemovePagingParametersFromRecordQuery(input);
 
            var responseContent = new PushStreamContent(
                async (stream, content, context) =>
                {
                    using (stream)
                    {
                        var records = recordQueryer.Query(input).ToList();
                        var writeHeaders = true;
                        foreach (var record in records)
                        {
                            var writer = new StringWriter();
                            var exporter = new Exporter();
                            if (writeHeaders)
                            {
                                exporter.ExportHeader(writer, delimiter);
                                writeHeaders = false;
                            }

                            exporter.ExportRecord(Helpers.RemoveCollectionFromId(record), writer, delimiter);
                            var data = Encoding.UTF8.GetBytes(writer.ToString());

                            await stream.WriteAsync(data, 0, data.Length);
                        }
                    }
                });

            return responseContent;
        }
    }
}
