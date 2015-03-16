using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Raven.Client;

namespace Catalogue.Web.Controllers.Dumps
{
    public class DumpsController : ApiController
    {
        readonly IDocumentSession db;

        public DumpsController(IDocumentSession db)
        {
            this.db = db;
        }

        [HttpGet, Route("api/dumps/allkeywordsinrecords")]
        public List<Tuple<string, string>> AllKeywordsInRecords()
        {
            var q = db.Query<RecordKeywordIndex.Result, RecordKeywordIndex>();

            return q.ToList().Select(k => Tuple.Create(k.Vocab, k.Value)).ToList();
        }
    }
}
