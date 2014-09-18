using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Data.Indexes;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Web.Admin.Vocabularies
{
    public interface IVocabulariesRepository
    {
        ICollection<string> Read(string vocab);
    }

    public class VocabulariesRepository : IVocabulariesRepository
    {
        private readonly IDocumentSession _db;

        public VocabulariesRepository(IDocumentSession db)
        {
            _db = db;
        }

        public ICollection<string> Read(string vocab)
        {
            if (String.IsNullOrWhiteSpace(vocab)) return new List<string>();

            var containsTerm = "*" + vocab.Trim().Replace("*", String.Empty) + "*";

            return _db.Query<VocabularySearchIndex.Result, VocabularySearchIndex>()
                .Search(k => k.Vocab, containsTerm, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards)
                .Select(k => k.Vocab)
                .Distinct().ToList();

        }
    }
}