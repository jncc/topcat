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

        public ICollection<MetadataKeyword> Get(string q)
        {
            return FindKeywords(q);
        }

        public ICollection<MetadataKeyword> FindKeywords(string value)
        {
            int startA = 0;
            var keywords = new List<MetadataKeyword>();

            //Get matching keywords from Vocab table
            //Do this to get around the limit on max number of results returned by raven
            while (VocabBaseQuery(startA).Any(k => k.Value.StartsWith(value)))
            {
                List<MetadataKeyword> current = VocabBaseQuery(startA).Where(k => k.Value.StartsWith(value)).Select(r => new MetadataKeyword { Value = r.Value, Vocab = r.Vocab })
                    .ToList();
                startA += current.Count;
                keywords.AddRange(current);
            }

            var startB = 0;
            //Get misc keywords from Records table
            while (MiscBaseQuery(startB).Any(k => k.Value.StartsWith(value)))
            {
                var current =
                    MiscBaseQuery(startB)
                        .Where(k => k.Value.StartsWith(value))
                        .Select(r => new MetadataKeyword { Value = r.Value, Vocab = r.Vocab })
                        .ToList();

                startB += current.Count;
                keywords.AddRange(current);
            }

            return keywords;
        }

        IQueryable<VocabularyKeywordIndex.Result> VocabBaseQuery(int skip)
        {
            return db.Query<VocabularyKeywordIndex.Result, VocabularyKeywordIndex>()
                .Skip(skip)
                .Take(1024);
        }

        IQueryable<KeywordsSearchIndex.Result> MiscBaseQuery(int skip)
        {
            return db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
                      .Where(x => x.Vocab == String.Empty)
                      .Skip(skip)
                      .Take(1024);
        }


    }


}
