using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using Moq;
using Raven.Client;
using NUnit.Framework;

namespace Catalogue.Data.Write
{
    public interface IVocabularyService
    {
        VocabularyServiceResult Insert(Vocabulary vocab);
        VocabularyServiceResult Update(Vocabulary vocab);
        void Import(List<MetadataKeyword> keywords);
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

        public void Import(List<MetadataKeyword> keywords)
        {
            // really specifically for import, adds the keywords
            foreach (var source in SeparateKeywordsIntoVocabularies(keywords))
            {
                var vocabulary = db.Load<Vocabulary>(source.Id);

                if (vocabulary == null)
                {
                    Insert(source);
                }
                else
                {
                    // the vocab already exists, so just add any new keywords
                    var newKeywords = source.Keywords.Except(vocabulary.Keywords);
                    vocabulary.Keywords.AddRange(newKeywords);
                    db.Store(vocabulary);
                }
            }
        }

        /// <summary>
        /// Creates Vocabulary objects given a bunch of keywords. For importing.
        /// </summary>
        List<Vocabulary> SeparateKeywordsIntoVocabularies(List<MetadataKeyword> keywords)
        {
            var q = from k in keywords
                    where k.Vocab.IsNotBlank() // only want keywords with vocabs, obviously
                    group k by k.Vocab into g // groups of keywords (vocabs)
                    select new Vocabulary
                    {
                        Id = g.Key,
                        Name = g.Key,
                        Description = String.Empty,
                        PublicationDate = DateTime.Now.ToString("yyyy-MM"),
                        Publishable = false,
                        Keywords = g.Select(k => k.Value)
                            .Distinct()
                            .Select(v => new VocabularyKeyword { Value = v }).ToList()
                    };

            return q.ToList();
        }

        internal VocabularyServiceResult Upsert(Vocabulary vocab)
        {
            db.Store(vocab);

            return new VocabularyServiceResult
            {
                Success = true,
                Vocab = vocab,
//              Validation = validationResult
            };
        }



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
