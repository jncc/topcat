using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.Keywords
{
    public class KeywordsController : ApiController
    {
        readonly IDocumentSession db;

        public KeywordsController(IDocumentSession db)
        {
            this.db = db;
        }

        public List<MetadataKeyword> Get(string q, int take = 10)
        {
            var vocabfulKeywords = db.Query<VocabularyKeywordIndex.Result, VocabularyKeywordIndex>()
                .Search(k => k.Value, q)
                .Take(take)
                .Select(r => new { r.Vocab, r.Value })
                .ToList();

//            var vocablessKeywords = new List<MetadataKeyword>();
            var vocablessKeywords = db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
                .Search(k => k.Value, q)
                .Take(take)
                .Select(r => new { r.Vocab, r.Value })
                .ToList();

            return vocabfulKeywords.Concat(vocablessKeywords.Except(vocabfulKeywords))
                .Select(x => new MetadataKeyword { Vocab = x.Vocab, Value = x.Value })
                .Take(take)
                .ToList();
        }
    }
}
