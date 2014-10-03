using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Web.Admin.Keywords
{
    public interface IKeywordsRepository
    {
        Keyword Create(String value, String vocab);
        void Delete(Keyword keyword);
        ICollection<Keyword> Read(String value, String vocab);
        ICollection<Keyword> ReadByVocab(string vocab);
        ICollection<Keyword> ReadByValue(string value);
        ICollection<Keyword> ReadAll();
    }

    public class KeywordsRepository : IKeywordsRepository
    {
        private readonly IDocumentSession _db;


        public KeywordsRepository(IDocumentSession db)
        {
            _db = db;
        }

        public Keyword Create(string value, string vocab)
        {
            throw new NotImplementedException();
        }

        public void Delete(Keyword keyword)
        {
            throw new NotImplementedException();
        }

        public ICollection<Keyword> Read(string value = null, string vocab = null)
        {
            throw new NotImplementedException();
        }

        public ICollection<Keyword> ReadByVocab(string vocab)
        {
            throw new NotImplementedException();
        }

        public ICollection<Keyword> ReadByValue(string value)
        {
            int startA = 0;
            var keywords = new List<Keyword>();

            //Get matching keywords from Vocab table
            //Do this to get around the limit on max number of results returned by raven
            while (VocabBaseQuery(startA).Any(k => k.Value.StartsWith(value)))
            {
                List<Keyword> current = VocabBaseQuery(startA).Where(k => k.Value.StartsWith(value)).Select(r => new Keyword {Value = r.Value, Vocab = r.Vocab})
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
                        .Select(r => new Keyword {Value = r.Value, Vocab = r.Vocab})
                        .ToList();

                startB += current.Count;
                keywords.AddRange(current);
            }

            return keywords;
        }

        public ICollection<Keyword> ReadAll()
        {
            int start = 0;
            var keywords = new List<Keyword>();
            while (VocabBaseQuery(start).Any())
            {
                List<Keyword> current = VocabBaseQuery(start).Select(r => new Keyword {Value = r.Value, Vocab = r.Vocab})
                        .ToList();
                start += current.Count;
                keywords.AddRange(current);
            }


            return keywords;
        }

        private IQueryable<VocabularyKeywordIndex.Result> VocabBaseQuery(int start)
        {
            return _db.Query<VocabularyKeywordIndex.Result, VocabularyKeywordIndex>()
                .Skip(start)
                .Take(1024);
        }

        private IQueryable<KeywordsSearchIndex.Result> MiscBaseQuery(int start)
        {
            return _db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
                      .Where(x => x.Vocab == String.Empty)
                      .Skip(start)
                      .Take(1024);
        }

    }
}