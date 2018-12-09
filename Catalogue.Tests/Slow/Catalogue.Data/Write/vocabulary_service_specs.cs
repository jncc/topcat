using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Moq;
using NUnit.Framework;
using Raven.Client.Documents.Session;

namespace Catalogue.Tests.Slow.Catalogue.Data.Write
{
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
            database.Setup(x => x.Load<Vocabulary>("")).Returns<Vocabulary>(null);

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
