using System;
using System.Linq;
using System.Collections.Generic;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Gemini.Write
{
    public interface IVocabularyService
    {
        Vocabulary Load(string id);
        VocabularyServiceResult Upsert(Vocabulary vocab);
    }

    public class VocabularyService : IVocabularyService
    {
        readonly IDocumentSession db;

        public VocabularyService(IDocumentSession db)
        {
            this.db = db;
        }

        public Vocabulary Load(string id)
        {
            return db.Load<Vocabulary>(id);
        }

        public VocabularyServiceResult Upsert(Vocabulary vocab)
        {
            vocab.Values = vocab.Values.Distinct().ToList();

            db.Store(vocab);

            return new VocabularyServiceResult
                {
                    vocab = vocab
                };
        }


    }

    public class VocabularyServiceResult
    {
        public Vocabulary vocab { get; set; }
    }
}
