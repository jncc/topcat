using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Data.Model;
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
            // todo use the RecordQueryer instead, and/or think about combining with ExportController??
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