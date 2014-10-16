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

            var uriResult = ValidateVocabularyUri(vocab.Id);
            if (uriResult.Success == false) return uriResult;

            vocab.Values = vocab.Values.Distinct().ToList();

            var existingVocab = Load(vocab.Id);

            if (existingVocab == null)
            {
                CleanKeywords(vocab);

                db.Store(vocab);
            }
            else if (!existingVocab.Controlled)
            {
                existingVocab.Values.AddRange(
                    vocab.Values.Where(v => existingVocab.Values.All(x => x != v)).Select(v => v));

                CleanKeywords(existingVocab);

                db.Store(existingVocab);
            }
            else if (vocab.Values.Any(v => existingVocab.Values.All(x => x != v)))
            {
                return new VocabularyServiceResult()
                    {
                        Success = false,
                        Error = String.Format("Cannot update the vocabulary {0} as it is controlled", vocab.Id)
                    };

            }

            return new VocabularyServiceResult
                {
                    Success = true,
                    Error = String.Empty
                };
        }

        private void CleanKeywords(Vocabulary vocab)
        {
            vocab.Values =
                vocab.Values.Select(x => x.Trim())
                     .Distinct(StringComparer.CurrentCultureIgnoreCase)
                     .OrderBy(x => x)
                     .ToList();
        }

        private VocabularyServiceResult ValidateVocabularyUri(string id)
        {
            Uri url;

            if (Uri.TryCreate(id, UriKind.Absolute, out url))
            {
                if (url.Scheme != Uri.UriSchemeHttp)
                {
                    return new VocabularyServiceResult
                    {
                        Success = false,
                        Error = String.Format("Resource locator {0} is not an http url", id)
                    };
                }
            }
            else
            {
                return new VocabularyServiceResult
                {
                    Success = false,
                    Error = String.Format("Resource locator {0} is not a valid url", id)
                };
            }

            return new VocabularyServiceResult
                {
                    Success = true,
                    Error = String.Empty
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
                            PublicationDate = DateTime.Now.ToString("MM-yyyy"),
                            Publishable = true,
                            Values =
                                keywords.Where(k => k.Vocab == vocabId)
                                        .Select(k => k.Value)
                                        .ToList()
                        }
                    into vocab
                    select UpsertVocabulary(vocab)).ToList();
        }
    }

    

    public class VocabularyServiceResult
    {
        public bool Success { get; set; }

        public string Error { get; set; }
    }


}
