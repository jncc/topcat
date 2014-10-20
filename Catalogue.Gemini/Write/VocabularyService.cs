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

        public VocabularyService(IDocumentSession db)
        {
            this.db = db;
        }

        public Vocabulary Load(string id)
        {
            return db.Load<Vocabulary>(id);
        }

        private VocabularyServiceResult UpsertVocabulary(Vocabulary vocab)
        {
            var uriResult = ValidateVocabularyUri(vocab.Id);
            if (uriResult != String.Empty)
                return new VocabularyServiceResult
                    {
                        Success = false,
                        Vocab = vocab,
                        ValidationError = uriResult
                    };

            vocab.Values = vocab.Values.Distinct().ToList();

            var existingVocab = Load(vocab.Id);

            if (existingVocab == null)
            {
                RemoveDuplicateKeywords(vocab);

                db.Store(vocab);
            }
            else
            {
                //merge values
                existingVocab.Values.AddRange(
                    vocab.Values.Where(v => existingVocab.Values.All(x => x != v)).Select(v => v));

                RemoveDuplicateKeywords(existingVocab);

                //Update vocab
                existingVocab.Name = vocab.Name;
                existingVocab.Description = vocab.Description;
                existingVocab.PublicationDate = vocab.PublicationDate;
                existingVocab.Publishable = vocab.Publishable;
                existingVocab.Controlled = vocab.Controlled;

                db.Store(existingVocab);
            }

            return new VocabularyServiceResult
                {
                    Success = true,
                    Vocab = vocab,
                    ValidationError = String.Empty
                };
        }

        private void RemoveDuplicateKeywords(Vocabulary vocab)
        {
            vocab.Values =
                vocab.Values.Select(x => x.Trim())
                     .Distinct(StringComparer.CurrentCultureIgnoreCase)
                     .OrderBy(x => x)
                     .ToList();
        }

        private String ValidateVocabularyUri(string id)
        {
            Uri url;

            if (String.IsNullOrWhiteSpace(id))
            {
                return "A vocabulary must have a properly formed Id";
            }

            if (Uri.TryCreate(id, UriKind.Absolute, out url))
            {
                if (url.Scheme != Uri.UriSchemeHttp)
                {

                    return String.Format("Resource locator {0} is not an http url", id);
                }
            }
            else
            {
                return String.Format("Resource locator {0} is not a valid url", id);
            }

            return String.Empty;
        }

        private VocabularyServiceResult UpsertKeywords(Vocabulary vocab)
        {
            var uriResult = ValidateVocabularyUri(vocab.Id);
            if (uriResult != String.Empty)
                return new VocabularyServiceResult
                {
                    Success = false,
                    Vocab = vocab,
                    ValidationError = uriResult
                };

            vocab.Values = vocab.Values.Distinct().ToList();

            var existingVocab = Load(vocab.Id);

            //Upsert keywords for new and non controlled vocabularies.
            if (existingVocab == null)
            {
                RemoveDuplicateKeywords(vocab);

                db.Store(vocab);
            }
            else if (!existingVocab.Controlled)
            {
                existingVocab.Values.AddRange(
                    vocab.Values.Where(v => existingVocab.Values.All(x => x != v)).Select(v => v));

                RemoveDuplicateKeywords(existingVocab);

                db.Store(existingVocab);
            }
            else if (vocab.Values.Any(v => existingVocab.Values.All(x => x != v)))
            {
                return new VocabularyServiceResult()
                {
                    Success = false,
                    Vocab = vocab,
                    ValidationError = String.Format("Cannot update the vocabulary {0} as it is controlled", vocab.Id)
                };

            }

            return new VocabularyServiceResult
            {
                Success = true,
                Vocab = vocab,
                ValidationError = String.Empty
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
                            Values =
                                keywords.Where(k => k.Vocab == vocabId)
                                        .Select(k => k.Value)
                                        .ToList()
                        }
                    into vocab
                    select UpsertKeywords(vocab)).ToList();
        }

        public VocabularyServiceResult Insert(Vocabulary vocab)
        {
            var uriResult = ValidateVocabularyUri(vocab.Id);
            if (uriResult != String.Empty)
            {
                return new VocabularyServiceResult
                    {
                        Success = false,
                        Vocab = vocab,
                        ValidationError = uriResult
                    };
            }

            if (Load(vocab.Id) != null)
            {
                return new VocabularyServiceResult
                    {
                        Success = false,
                        Vocab = vocab,
                        ValidationError = String.Format("A vocabulary with id {0} already exists", vocab.Id)
                    };
            }

            return UpsertVocabulary(vocab);
        }

        public VocabularyServiceResult Update(Vocabulary vocab)
        {
            var uriResult = ValidateVocabularyUri(vocab.Id);
            if (uriResult != String.Empty)
            {
                return new VocabularyServiceResult
                    {
                        Success = false,
                        Vocab = vocab,
                        ValidationError = uriResult
                    };
            }

            return UpsertVocabulary(vocab);
        }
    }


    public class VocabularyServiceResult
    {
        public Vocabulary Vocab { get; set; }
        public bool Success { get; set; }
        public string ValidationError { get; set; }
    }
}
