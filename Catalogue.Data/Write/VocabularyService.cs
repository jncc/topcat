using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private readonly IVocabularyValidator validator;

        private readonly IDocumentSession db;
//        private readonly IVocabularyValidator validator;

        public VocabularyService(IDocumentSession db, IVocabularyValidator validator)
        {
            this.db = db;
            this.validator = validator;
        }



        public VocabularyServiceResult Insert(Vocabulary vocab)
        {
            //Only insert new vocabs
            var existingVocab = db.Load<Vocabulary>(vocab.Id);

            if (existingVocab != null) throw new InvalidOperationException("Cannot insert an existing record.");
               
            return Upsert(vocab);          
        }

        public VocabularyServiceResult Update(Vocabulary vocab)
        {
            return Upsert(vocab);
        }

        public void Import(List<MetadataKeyword> keywords)
        {
            // (specifically for imports) adds the keywords to existing controlled vocabs
            foreach (var source in SeparateKeywordsIntoVocabularies(keywords))
            {
                var vocabulary = db.Load<Vocabulary>(source.Id);

                if (vocabulary != null && vocabulary.Controlled)
                {
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

        public VocabularyServiceResult Upsert(Vocabulary vocab)
        {
            vocab.Keywords = (from k in vocab.Keywords
                             group k by k.Value.ToLower() into g
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

    [TestFixture]
    public class when_upserting_a_vocabulary
    {

        private Mock<IDocumentSession> database;
        private Mock<IVocabularyValidator> validator;
        private VocabularyService service;

        Vocabulary BasicVocabulary()
        {
            return new Vocabulary
            {
                Id = "http://some/vocab",
                Name = "Some Name",
                Keywords = new List<VocabularyKeyword>()
            };
        }


        [SetUp]
        public void Init()
        {
            database = new Mock<IDocumentSession>();
            //mock existing vocab test
            database.Setup(x => x.Load<Vocabulary>()).Returns<Vocabulary>(null);

            validator = new Mock<IVocabularyValidator>();
            validator.Setup(x => x.Validate(It.IsAny<Vocabulary>())).Returns(new ValidationResult<Vocabulary>());

            service = new VocabularyService(database.Object, validator.Object);
        }

        [Test]
        public void should_validate_and_store_record_in_the_database()
        {
            var record = BasicVocabulary();
            service.Upsert(record);

            Mock.Get(validator.Object).Verify(v => v.Validate(record));
            Mock.Get(database.Object).Verify(db => db.Store(record));
        }


        [Test]
        public void should_not_duplicate_keywords()
        {
            var record = BasicVocabulary();
            record.Keywords = new List<VocabularyKeyword>
            {
                new VocabularyKeyword() {Value = "TestA"},
                new VocabularyKeyword() {Value = "TestDupe"},
                new VocabularyKeyword() {Value = "TestDupe"}
            };

            Vocabulary savedVocab = null;

            database.Setup(x => x.Store(It.IsAny<Vocabulary>())).Callback<Object>((v) => savedVocab = (Vocabulary)v);

            service.Upsert(record);

            Assert.That(savedVocab.Keywords.Count, Is.EqualTo(2));
            Assert.That(savedVocab.Keywords.Any(x => x.Value == "TestA"), Is.EqualTo(true));
            Assert.That(savedVocab.Keywords.Any(x => x.Value == "TestDupe"), Is.EqualTo(true));
        }

        [Test]
        public void should_order_keywords_alphabetically()
        {
            var record = BasicVocabulary();
            record.Keywords = new List<VocabularyKeyword>
            {
                new VocabularyKeyword() {Value = "Z"},
                new VocabularyKeyword() {Value = "C"},
                new VocabularyKeyword() {Value = "A"}
            };

            Vocabulary savedVocab = null;

            database.Setup(x => x.Store(It.IsAny<Vocabulary>())).Callback<Object>((v) => savedVocab = (Vocabulary)v);

            service.Upsert(record);

            Assert.That(savedVocab.Keywords.Count, Is.EqualTo(3));
            Assert.That(savedVocab.Keywords[0].Value, Is.EqualTo("A"));
            Assert.That(savedVocab.Keywords[1].Value, Is.EqualTo("C"));
            Assert.That(savedVocab.Keywords[2].Value, Is.EqualTo("Z"));
        }



    }

}
