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
        ICollection<Keyword> ReadAllWithoutIndex();
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
            int start = 0;
            var keywords = new List<Keyword>();
            while (BaseQuery(start).Any(k => k.Value.StartsWith(value)))
            {
                List<Keyword> current = BaseQuery(start).Where(k => k.Value.StartsWith(value)).Select(r => new Keyword {Value = r.Value, Vocab = r.Vocab})
                    .ToList(); 
                start += current.Count;
                keywords.AddRange(current);
            }
            return keywords;
        }

        public ICollection<Keyword> ReadAll()
        {
            int start = 0;
            var keywords = new List<Keyword>();
            while (BaseQuery(start).Any())
            {
                List<Keyword> current = BaseQuery(start).Select(r => new Keyword {Value = r.Value, Vocab = r.Vocab})
                        .ToList();
                start += current.Count;
                keywords.AddRange(current);
            }
            return keywords;
        }

        public ICollection<Keyword> ReadAllWithoutIndex()
        {
            int start = 0;
            var allKeywords = new HashSet<Keyword>();
            var allRecords = new List<Record>();
            while (true)
            {
                List<Record> current = _db.Query<Record>().Take(1024).Skip(start).ToList();

                if (current.Count == 0)
                    break;

                start += current.Count;

                /* The only way to access non-root documents, is via the root document - ravendb is not a relational db*/

                foreach (Record record in current)
                {
                    List<Keyword> keywords = record.Gemini.Keywords.ToList();
                    allKeywords.UnionWith(keywords);
                }
                allRecords.AddRange(current);
            }
            return allKeywords;
        }

        private IQueryable<KeywordsSearchIndex.Result> BaseQuery(int start)
        {
            return _db.Query<KeywordsSearchIndex.Result, KeywordsSearchIndex>()
                .Skip(start)
                .Take(1024);
        }
    }
}