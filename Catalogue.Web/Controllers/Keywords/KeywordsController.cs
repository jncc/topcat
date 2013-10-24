using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.Keywords
{
    public class KeywordsController : ApiController
    {
        public List<Keyword> Get(string s)
        {
            using (var db = WebApiApplication.DocumentStore.OpenSession())
            {
                var results = db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
                  .Search(r => r.Value, s)
                  .Search(r => r.ValueN, s)
                  .Take(5).ToList();

                return results.Select(r => new Keyword { Vocab = r.Vocab, Value = r.Value }).ToList();
            }
        }
    }
}
