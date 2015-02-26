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
//                .Highlight(r => r.Value, 202, 1, out vocabfulLites)
//                .SetHighlighterTags("<b>", "</b>")
//                .Customize(x=>x.Highlight("Value", 128, 1, out vocabfulLites))
                .Search(k => k.Value, q)
                .Take(take)
                .Select(r => new { r.Vocab, r.Value })
                .ToList();

            var vocablessKeywords = db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
                .Search(k => k.Value, q)
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
