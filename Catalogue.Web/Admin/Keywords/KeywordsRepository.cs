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
            IQueryable<Keyword> query = _db.Query<Keyword>();
            return query.ToList();
        }
    }
}