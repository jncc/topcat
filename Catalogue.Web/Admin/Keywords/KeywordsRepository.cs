using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Web.Admin.Keywords
{
    public interface IKeywordsRepository
    {
        Keyword Create(String value, String vocab);
        void Delete(Keyword keyword);
        List<Keyword> Read(String value, String vocab);
        List<Keyword> ReadAllByVocab(string vocab);
        List<Keyword> ReadAllByValue(string value);
        List<Keyword> ReadAll();
    }
    public class KeywordsRepository : IKeywordsRepository
    {  private readonly IDocumentSession _db;


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

        public List<Keyword> Read(string value = null, string vocab = null)
        {
            throw new NotImplementedException();
        }

        public List<Keyword> ReadAllByVocab(string vocab)
        {
            throw new NotImplementedException();
        }

        public List<Keyword> ReadAllByValue(string value)
        {
            throw new NotImplementedException();
        }

        public List<Keyword> ReadAll()
        {
            int start = 0;
            var allKeywords = new List<Keyword>();
            while (true)
            {
                var current = _db.Query<Keyword>("KeywordIndex").Take(1024).Skip(start).ToList();
                if (current.Count == 0)
                    break;

                start += current.Count;
                allKeywords.AddRange(current);

            }
            return allKeywords;
        }
    }
}