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
        VocabularyServiceResult UpsertVocabulary(Vocabulary vocab);
        ICollection<VocabularyServiceResult> SyncKeywords(List<Keyword> keywords);
    }

    public class VocabularyService : IVocabularyService
    {
        private readonly IDocumentSession db;

        public VocabularyService(IDocumentSession db)
        {
            this.db = db;
        }

        public Vocabulary Load(string id)
        {
            return db.Load<Vocabulary>(id);
        }

        public VocabularyServiceResult UpsertVocabulary(Vocabulary vocab)
        {
            if (String.IsNullOrWhiteSpace(vocab.Id))
            {
                throw new InvalidOperationException("Cannot upsert a vocabaulary with no id");
            }

            vocab.Values = vocab.Values.Distinct().ToList();

            var existingVocab = Load(vocab.Id);

            if (existingVocab == null)
            {
                db.Store(vocab);
            }
            else if (!existingVocab.Controlled)
            {
                existingVocab.Values.AddRange(
                    vocab.Values.Where(v => existingVocab.Values.All(x => x != v)).Select(v => v));

                db.Store(existingVocab);
            }
            else if (vocab.Values.Any(v => existingVocab.Values.All(x => x != v)))
            {
                throw new InvalidOperationException("Cannot implicitly update controlled vocabularies");

            }

            return new VocabularyServiceResult
                {
                    Success = true,
                };
        }

        public ICollection<VocabularyServiceResult> SyncKeywords(List<Keyword> keywords)
        {
            if (keywords == null) return new List<VocabularyServiceResult>();

            return (from vocabId in keywords.Select(k => k.Vocab)
                    where !String.IsNullOrWhiteSpace(vocabId)
                    select new Vocabulary
                        {
                            Id = vocabId, 
                            Controlled = false, 
                            Name = vocabId, 
                            Description = String.Empty, 
                            PublicationDate = DateTime.Now.ToString("mm-yyyy"), 
                            Publishable = true, 
                            Values = keywords.Where(k => k.Vocab == vocabId).Select(k => k.Value).ToList()
                        }
                    into vocab select UpsertVocabulary(vocab)).ToList();
        }
    }

    

    public class VocabularyServiceResult
    {
        public bool Success { get; set; }
    }


}
