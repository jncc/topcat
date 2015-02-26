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
                .Where(k => k.Value.StartsWith(q))
                .Take(take)
                .Select(r => new MetadataKeyword { Value = r.Value, Vocab = r.Vocab })
                .ToList();

            var vocablessKeywords = db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
                .Where(x => x.Vocab == String.Empty)
                .Where(k => k.Value.StartsWith(q))
                .Take(take)
                .Select(r => new MetadataKeyword { Value = r.Value, Vocab = r.Vocab })
                .ToList();

            return vocabfulKeywords.Concat(vocablessKeywords)
                .Take(take)
                .ToList();
        }
    }
}
