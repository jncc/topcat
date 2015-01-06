using Catalogue.Gemini.Model;
using Moq;
using Raven.Client;
using NUnit.Framework;

namespace Catalogue.Data.Write
{
    public interface IVocabularyService
    {
//        ICollection<VocabularyServiceResult> AddKeywords(List<MetadataKeyword> keywords);
        VocabularyServiceResult Insert(Vocabulary vocab);
        VocabularyServiceResult Update(Vocabulary vocab);
    }

    public class VocabularyService : IVocabularyService
    {
        private readonly IDocumentSession db;
//        private readonly IVocabularyValidator validator;

        public VocabularyService(IDocumentSession db)
        {
            this.db = db;
//            this.validator = validator;
        }

        public VocabularyServiceResult Insert(Vocabulary vocab)
        {
            return Upsert(vocab);
        }

        public VocabularyServiceResult Update(Vocabulary vocab)
        {
            return Upsert(vocab);
        }

        internal VocabularyServiceResult Upsert(Vocabulary vocab)
        {
            db.Store(vocab);

            return new VocabularyServiceResult
            {
                Success = true,
                Vocab = vocab,
//                    Validation = validationResult
            };
        }



//        public ICollection<VocabularyServiceResult> AddKeywords(List<MetadataKeyword> keywords)
//        {
//            if (keywords == null) return new List<VocabularyServiceResult>();
//
//            return (from vocabId in keywords.Select(k => k.Vocab)
//                    where !String.IsNullOrWhiteSpace(vocabId)
//                    select new Vocabulary
//                        {
//                            Id = vocabId,
//                            Controlled = false,
//                            Name = vocabId,
//                            Description = String.Empty,
//                            PublicationDate = DateTime.Now.ToString("MM-yyyy"),
//                            Publishable = true,
//                            Keywords =
//                                keywords.Where(k => k.Vocab == vocabId)
//                                        .Select(k => new VocabularyKeyword{Id = Guid.NewGuid(), Value = k.Value})
//                                        .ToList()
//                        }
//                    into vocab
//                    select LimitedVocabularyUpsert(vocab)).ToList();
//        }

    }


    public class VocabularyServiceResult
    {
        public Vocabulary Vocab { get; set; }
        public bool Success { get; set; }
//        public VocabularyValidationResult  Validation { get; set; }
    }

    public class when_upserting_a_vocabulary
    {
        [Test]
        public void should_store_record_in_the_database()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new VocabularyService(database);

            var record = BasicVocabulary();
            service.Upsert(record);

            Mock.Get(database).Verify(db => db.Store(record));
        }

        Vocabulary BasicVocabulary()
        {
            return new Vocabulary
            {
                // todo
            };
        }
    }

}
