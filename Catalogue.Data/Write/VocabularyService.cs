using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using Raven.Client.Documents.Session;

namespace Catalogue.Data.Write
{
    public interface IVocabularyService
    {
        VocabularyServiceResult Insert(Vocabulary vocab);
        VocabularyServiceResult Update(Vocabulary vocab);
        void AddKeywordsToExistingControlledVocabs(List<MetadataKeyword> keywords);
    }

    public class VocabularyService : IVocabularyService
    {
        private readonly IVocabularyValidator validator;

        private readonly IDocumentSession db;

        public VocabularyService(IDocumentSession db, IVocabularyValidator validator)
        {
            this.db = db;
            this.validator = validator;
        }

        public VocabularyServiceResult Insert(Vocabulary vocab)
        {
            var existingVocab = db.Load<Vocabulary>(vocab.Id);

            if (existingVocab == null)
            {
                return Upsert(vocab);
            }
            else
            {
                var validation = new ValidationResult<Vocabulary>();
                validation.Errors.Add("Vocabulary already exists", v => v.Id);
                return new VocabularyServiceResult { Vocab = vocab, Validation =  validation };
            }
        }

        public VocabularyServiceResult Update(Vocabulary vocab)
        {
            return Upsert(vocab);
        }

        /// <summary>
        /// Specifically for imports. Adds the keywords to existing controlled vocabs.
        /// </summary>
        public void AddKeywordsToExistingControlledVocabs(List<MetadataKeyword> keywords)
        {
            foreach (var group in GroupKeywordsByVocabularyAndEnsureNoDuplicates(keywords))
            {
                var vocabulary = db.Load<Vocabulary>(group.Key);

                if (vocabulary != null && vocabulary.Controlled)
                {
                    var newKeywords = group.Value.Except(vocabulary.Keywords);
                    vocabulary.Keywords.AddRange(newKeywords);
                    Update(vocabulary);
                }
            }
        }

        Dictionary<string, List<VocabularyKeyword>> GroupKeywordsByVocabularyAndEnsureNoDuplicates(List<MetadataKeyword> keywords)
        {
            var q = from k in keywords
                    where k.Vocab.IsNotBlank() // only want keywords with vocabs, obviously
                    group k by k.Vocab into g // groups of keywords (vocabs)
                    let vocab = g.Key
                    let distinctKeywords = g.Select(k => k.Value)
                        .Distinct()
                        .Select(v => new VocabularyKeyword { Value = v })
                        .ToList()
                    select new { vocab, keywords = distinctKeywords};

            return q.ToDictionary(g => g.vocab, g => g.keywords);
        }

        public VocabularyServiceResult Upsert(Vocabulary vocab)
        {
            vocab.Keywords = (from k in vocab.Keywords
                             group k by k.Value.ToLower().Trim() into g // distinct by Value
                             select g.First() into n
                             orderby n.Value
                             select n).ToList();

            var validation = validator.Validate(vocab);

            if (!validation.Errors.Any()) db.Store(vocab);

            return new VocabularyServiceResult
            {
                Vocab = vocab,
                Validation = validation
            };
        }
    }


    public class VocabularyServiceResult
    {
        public Vocabulary Vocab { get; set; }
        public ValidationResult<Vocabulary> Validation { get; set; }
        public bool Success { get { return !Validation.Errors.Any(); } }
    }
}
