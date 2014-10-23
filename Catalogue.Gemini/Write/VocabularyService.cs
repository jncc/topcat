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
                    ValidationError = String.Empty
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
                    ValidationError = String.Empty
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
                    ValidationError = String.Empty
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
                Vocab = targetVocab,
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
