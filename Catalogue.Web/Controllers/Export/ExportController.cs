using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.UI.WebControls;
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

        public List<Record> Get(string k)
        {
            var keyword = ParameterHelper.ParseKeywords(new [] { k }).Single(); // for now, support only one

            var query = db.Query<Record>()
                .Where(r => r.Gemini.Keywords.Any(kw => kw.Value == keyword.Value && kw.Vocab == keyword.Vocab));

            return query.ToList();
        }

    }
}
