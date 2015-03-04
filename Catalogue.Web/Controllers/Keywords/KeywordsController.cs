using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Data.Indexes;
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

        public List<KeywordModel> Get(string q, int take = 10)
        {
            var vocabfulKeywords = db.Query<VocabularyKeywordIndex.Result, VocabularyKeywordIndex>()
                .Search(k => k.ValueN, q)
                .Take(take)
                .Select(r => new { r.Vocab, r.Value })
                .ToList();

            var vocablessKeywords = db.Query<RecordKeywordIndex.Result, RecordKeywordIndex>()
                .Search(k => k.ValueN, q)
                .Take(take)
                .Select(r => new { r.Vocab, r.Value })
                .ToList();

            return vocabfulKeywords.Concat(vocablessKeywords.Except(vocabfulKeywords))
                .Select(x => new KeywordModel { Vocab = x.Vocab, Value = x.Value })
                .Take(take)
                .ToList();
        }
    }
}
