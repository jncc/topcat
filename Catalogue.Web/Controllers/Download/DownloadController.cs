using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Web.Code.Account;
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

        public List<Record> Get(string k)
        {
            var keyword = ParameterHelper.ParseKeywords(new[] { k }).Single(); // for now, support only one

            var q = from r in db.Query<Record>()
                    where r.Gemini.Keywords.Any(x => x.Vocab == keyword.Vocab && x.Value == keyword.Value)
                    select r;

            return q.ToList();
        }

//        [HttpGet, Route("api/download/iso")]
//        public object Xml(RecordQueryInputModel input)
//        {
//            var keyword = ParameterHelper.ParseKeywords(new[] { input..K }).Single(); // for now, support only one
//
//        }
    }
}