using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using Raven.Client.Documents.Session;

namespace Catalogue.Data.Query
{
    public interface IVocabQueryer
    {
        Vocabulary GetVocab(string id);
    }

    public class VocabQueryer : IVocabQueryer
    {
        private readonly IDocumentSession db;

        public VocabQueryer(IDocumentSession db)
        {
            this.db = db;
        }

        public Vocabulary GetVocab(string id)
        {
            return db.Load<Vocabulary>(id);
        }
    }
}
