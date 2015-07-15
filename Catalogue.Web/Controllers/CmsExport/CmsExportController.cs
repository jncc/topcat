using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Newtonsoft.Json;
using Raven.Client;
using Raven.Client.Document.Async;

namespace Catalogue.Web.Controllers.CmsExport
{
    public class CmsExportController : ApiController
    {
        private IAsyncDocumentSession _db;

        public CmsExportController(IAsyncDocumentSession db)
        {
            this._db = db;
        }

        public HttpResponseMessage Post([FromUri] RecordQueryInputModel input)
        {
            HttpResponseMessage response = Request.CreateResponse();
            response.Content = new PushStreamContent(
                async (stream,  content,  context) =>
                {
                    using (stream)
                    using (var enumerator = await _db.Advanced.StreamAsync(AsyncRecordQuerier.RecordQuery(_db,input)))
                    {
                        while (await enumerator.MoveNextAsync())
                        {
                            var serialized = JsonConvert.SerializeObject(enumerator.Current);
                            var data = UTF8Encoding.UTF8.GetBytes(serialized);
                            var countPrefix = BitConverter.GetBytes(data.Length);
                            await stream.WriteAsync(countPrefix, 0, countPrefix.Length);
                            await stream.WriteAsync(data, 0, data.Length);
                        }
                    }
                });
            return response;
        }
    }
}
