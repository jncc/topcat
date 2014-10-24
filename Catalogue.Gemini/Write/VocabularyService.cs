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
        ICollection<VocabularyServiceResult> UpdateKeywords(List<MetadataKeyword> keywords);
        VocabularyServiceResult Insert(Vocabulary vocab);
        VocabularyServiceResult Update(Vocabulary vocab);
    }

    public class VocabularyService : IVocabularyService
    {
        private readonly IDocumentSession db;
        private readonly IVocabularyValidator validator;

        public VocabularyService(IDocumentSession db, IVocabularyValidator validator)
        {
            this.db = db;
            this.validator = validator;
        }

        public Vocabulary Load(string id)
        {
            return db.Load<Vocabulary>(id);
        }

        private VocabularyServiceResult UpsertVocabulary(Vocabulary vocab, VocabularyValidationResult validationResult)
        {

            vocab.Keywords = vocab.Keywords.Distinct().ToList();

            var targetVocab = Load(vocab.Id);

            if (targetVocab == null)
            {
                vocab.Keywords = RemoveDuplicateKeywords(vocab.Keywords).ToList();

                db.Store(vocab);

                return new VocabularyServiceResult
                {
                    Success = true,
                    Vocab = vocab,
                    Validation = validationResult
                };
            }
            else
            {
                var targetKeywordHash = new HashSet<string>(targetVocab.Keywords.Select(y => y.Value),
                                                            StringComparer.InvariantCultureIgnoreCase);
                var targetIdHash = new HashSet<Guid>(targetVocab.Keywords.Select(y => y.Id));

                var sourceIdHash = new HashSet<Guid>(vocab.Keywords.Select(y => y.Id));

                //delete removed keywords
                //remove from metadata records
                targetVocab.Keywords.RemoveAll(x => !sourceIdHash.Contains(x.Id));

                //update existing keywords
                var updates = vocab.Keywords.Where(x => targetIdHash.Contains(x.Id));
                foreach (var keyword in updates)
                {
                    targetVocab.Keywords.Single(x => x.Id == keyword.Id).Value = keyword.Value;
                    //update metadata records
                }

                //add new keywords
                var newKeywords = RemoveDuplicateKeywords(vocab.Keywords.Where(
                    x => x.Id == Guid.Empty && !targetKeywordHash.Contains(x.Value)));

                foreach (var keyword in newKeywords)
                {
                    keyword.Id = Guid.NewGuid();
                    targetVocab.Keywords.Add(keyword);
                }

                //Update vocab
                targetVocab.Name = vocab.Name;
                targetVocab.Description = vocab.Description;
                targetVocab.PublicationDate = vocab.PublicationDate;
                targetVocab.Publishable = vocab.Publishable;
                targetVocab.Controlled = vocab.Controlled;

                db.Store(targetVocab);

                return new VocabularyServiceResult
                {
                    Success = true,
                    Vocab = targetVocab,
                    Validation = validationResult
                };
            }

  
        }

        private IEnumerable<VocabularyKeyword> RemoveDuplicateKeywords(IEnumerable<VocabularyKeyword> keywords)
        {
            return (from i in keywords
                    group i by i.Value.ToLowerInvariant()
                    into g
                    select g.OrderBy(p => p.Value).First());
        }


        private VocabularyServiceResult UpsertKeywords(Vocabulary vocab)
        {
            var validationResult = validator.Valdiate(vocab, allowControlledUpdates:false);

            if (validationResult.Errors.Any())
                return new VocabularyServiceResult
                {
                    Success = false,
                    Vocab = vocab,
                    Validation = validationResult
                };

            vocab.Keywords = vocab.Keywords.Distinct().ToList();

            var targetVocab = Load(vocab.Id);
            if (targetVocab == null)
            {
                vocab.Keywords = RemoveDuplicateKeywords(vocab.Keywords).ToList();

                db.Store(vocab);

                return new VocabularyServiceResult
                {
                    Success = true,
                    Vocab = vocab,
                    Validation = validationResult
                };
            }

            var targetKeywordHash = new HashSet<string>(targetVocab.Keywords.Select(y => y.Value),
                                                        StringComparer.InvariantCultureIgnoreCase);

            //Upsert keywords for new and non controlled vocabularies.
            if (!targetVocab.Controlled)
            {
                //add keywords

                targetVocab.Keywords.AddRange(
                    vocab.Keywords.Where(
                        x =>
                        !targetKeywordHash.Contains(x.Value)));

                db.Store(targetVocab);
            }
            else if (vocab.Keywords.Any(x => !targetKeywordHash.Contains(x.Value))) 
            {
                throw new InvalidOperationException("Cannot update controlled vocabulary");

            }

            return new VocabularyServiceResult
            {
                Success = true,
                Vocab = targetVocab,
                Validation = validationResult
            };

        }

        public ICollection<VocabularyServiceResult> UpdateKeywords(List<MetadataKeyword> keywords)
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
                            PublicationDate = DateTime.Now.ToString("MM-yyyy"),
                            Publishable = true,
                            Keywords =
                                keywords.Where(k => k.Vocab == vocabId)
                                        .Select(k => new VocabularyKeyword{Id = Guid.NewGuid(), Value = k.Value})
                                        .ToList()
                        }
                    into vocab
                    select UpsertKeywords(vocab)).ToList();
        }

        public VocabularyServiceResult Insert(Vocabulary vocab)
        {
            var validationResult = validator.Valdiate(vocab, allowControlledUpdates: true);

            if (validationResult.Errors.Any())
            return new VocabularyServiceResult
                {
                    Success = false,
                    Vocab = vocab,
                    Validation = validationResult
                };


            if (Load(vocab.Id) != null)
            {
                validationResult.Errors.Add(String.Format("A vocabulary with id {0} already exists", vocab.Id));
                return new VocabularyServiceResult
                {
                    Success = false,
                    Vocab = vocab,
                    Validation = validationResult
                };
            }

            return UpsertVocabulary(vocab, validationResult);

        }

        public VocabularyServiceResult Update(Vocabulary vocab)
        {
            var validationResult = validator.Valdiate(vocab, allowControlledUpdates: true);

            if (validationResult.Errors.Any())
                return new VocabularyServiceResult
                {
                    Success = false,
                    Vocab = vocab,
                    Validation = validationResult
                };

            return UpsertVocabulary(vocab, validationResult);
        }
    }


    public class VocabularyServiceResult
    {
        public Vocabulary Vocab { get; set; }
        public bool Success { get; set; }
        public VocabularyValidationResult  Validation { get; set; }
    }
}
