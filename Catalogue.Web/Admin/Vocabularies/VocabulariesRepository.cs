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
        ICollection<Vocabulary> Read(string id);
    }

    public class VocabulariesRepository : IVocabulariesRepository
    {
        private readonly IDocumentSession _db;

        public VocabulariesRepository(IDocumentSession db)
        {
            _db = db;
        }

        //Don't return values - these are managed by keyword controller
        public ICollection<Vocabulary> Read(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) return new List<Vocabulary>();

            var containsTerm = "*" + id.Trim().Replace("*", String.Empty) + "*";

            return _db.Query<Vocabulary>()
                .Search(x => x.Id, containsTerm, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards)
                .Select(x => x)
                .ToList();
        }
    }
}