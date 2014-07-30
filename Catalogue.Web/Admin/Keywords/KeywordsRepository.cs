using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        ICollection<Keyword> ReadAllByVocab(string vocab);
        ICollection<Keyword> ReadAllByValue(string value);
        ICollection<Keyword> ReadAll();
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

        public ICollection<Keyword> Read(string value = null, string vocab = null)
        {
            throw new NotImplementedException();
        }

        public ICollection<Keyword> ReadAllByVocab(string vocab)
        {
            throw new NotImplementedException();
        }

        public ICollection<Keyword> ReadAllByValue(string value)
        {
            throw new NotImplementedException();
        }


        public ICollection<Keyword> ReadAll()
        {
            int start = 0;
            var allKeywords = new HashSet<Keyword>();
            var allRecords = new List<Record>();
            while (true)
            {
                var current = _db.Query<Record>().Take(1024).Skip(start).ToList();

                if (current.Count == 0)
                    break;

                start += current.Count;

                /* The only way to access non-root documents, is via the root document - ravendb is not a relational db*/

                foreach (var record in current)
                {
                    List<Keyword> keywords = record.Gemini.Keywords.ToList();
                    allKeywords.UnionWith(keywords);

                }
                allRecords.AddRange(current);
            }
            return allKeywords;
        }
    }
}