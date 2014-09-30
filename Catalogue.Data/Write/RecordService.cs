using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Catalogue.Data.Model;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Spatial;
using Catalogue.Gemini.Templates;
using Catalogue.Gemini.Write;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using Catalogue.Utilities.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Data.Write
{
    public interface IRecordService
    {
        Record Load(Guid id);
        RecordServiceResult Insert(Record record);
        RecordServiceResult Update(Record record);
    }

    public class RecordService : IRecordService
    {
        private readonly IDocumentSession db;
        private readonly IRecordValidator validator;
        private readonly IVocabularyService vocabService;

        public RecordService(IDocumentSession db, IRecordValidator validator, IVocabularyService vocabService)
        {
            this.db = db;
            this.validator = validator;
            this.vocabService = vocabService;
        }

        public Record Load(Guid id)
        {
            return db.Load<Record>(id);
        }

        public RecordServiceResult Insert(Record record)
        {
            return Upsert(record);
        }

        public RecordServiceResult Update(Record record)
        {
            if (record.ReadOnly)
                throw new InvalidOperationException("Cannot update a read-only record.");

            return Upsert(record);
        }

        internal RecordServiceResult Upsert(Record record)
        {
            // currently only supporting dataset resource types
            record.Gemini.ResourceType = "dataset";

            CorrectlyOrderKeywords(record);
            StandardiseUnconditionalUseConstraints(record);

            var validation = validator.Validate(record);

            if (validation.Errors.Any())
            {
                return new RecordServiceResult
                    {
                        Record = record,
                        Validation = validation
                    };
            }

            SyncDenormalizations(record);

            var vocabSyncResults = vocabService.SyncKeywords(record.Gemini.Keywords);

            if (vocabSyncResults.All(x => x.Success))
            {
                db.Store(record);
                
                return new RecordServiceResult
                {
                    Record = record,
                    Validation = validation
                };
            }
            
            AppendVocabValidationErrors(validation.Errors,vocabSyncResults);
            return new RecordServiceResult
                {
                    Record = record,
                    Validation = validation,
                };
        }

        private void AppendVocabValidationErrors(RecordValidationIssueSet errors, ICollection<VocabularyServiceResult> vocabSyncResults)
        {
            foreach (var error in vocabSyncResults.Where(x => !x.Success).Select(x => x.Error))
            {
                errors.Add(error, r => r.Gemini.Keywords);
            }
        }


        void SyncDenormalizations(Record record)
        {
            // we store the bounding box as wkt so we can index it
            if (!BoundingBoxUtility.IsBlank(record.Gemini.BoundingBox))
                record.Wkt = BoundingBoxUtility.ToWkt(record.Gemini.BoundingBox);
        }

        void CorrectlyOrderKeywords(Record record)
        {
            record.Gemini.Keywords = record.Gemini.Keywords
                .OrderByDescending(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category")
                .ThenByDescending(k => k.Vocab.IsNotBlank())
                .ThenBy(k => k.Vocab)
                .ThenBy(k => k.Value)
                .ToList();
        }

        void StandardiseUnconditionalUseConstraints(Record record)
        {
            const string unconditional = "no conditions apply";

            if (record.Gemini.UseConstraints.IsNotBlank() && record.Gemini.UseConstraints.ToLowerInvariant().Trim() == unconditional)
                record.Gemini.UseConstraints = unconditional;
        }
    }

    public class RecordServiceResult
    {
        public RecordValidationResult Validation { get; set; }
        public bool Success { get { return !Validation.Errors.Any(); } }

        /// <summary>
        /// The (possibly modified) record that was submitted.
        /// </summary>
        public Record Record { get; set; }

        /// <summary>
        /// A convenience result for tests, etc.
        /// </summary>
        public static readonly RecordServiceResult SuccessfulResult = new RecordServiceResult { Validation = new RecordValidationResult() };
    }


    class when_inserting_a_record
    {
        [Test]
        public void should_create_a_new_guid()
        {
            // todo
        }
    }

    class when_updating_a_record
    {
        [Test]
        public void should_fail_when_record_is_readonly()
        {
            var service = new RecordService(Mock.Of<IDocumentSession>(), Mock.Of<IRecordValidator>(),Mock.Of<IVocabularyService>());
            var record = new Record { ReadOnly = true };

            service.Invoking(s => s.Update(record))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Cannot update a read-only record.");
        }
    }

    class when_upserting_a_record
    {
        [Test]
        public void should_store_valid_record_in_the_database()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub(), Mock.Of<IVocabularyService>());

            var record = BasicRecord();
            service.Upsert(record);

            Mock.Get(database).Verify(db => db.Store(record));
        }

        [Test]
        public void should_not_store_invalid_record_in_the_database()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, FailingValidatorStub(), Mock.Of<IVocabularyService>());

            service.Upsert(BasicRecord());

            Mock.Get(database).Verify(db => db.Store(It.IsAny<Record>()), Times.Never);
        }

        [Test]
        public void should_return_record_in_result()
        {
            // so we can pass the possibly modified record back to the client
            // without an unnecessary fetch from the database

            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub(), Mock.Of<IVocabularyService>());

            var record = BasicRecord();
            var result = service.Upsert(record);

            result.Record.Should().Be(record);
        }

        [Test]
        public void should_store_bounding_box_as_wkt()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub(), Mock.Of<IVocabularyService>());
            
            var e = Library.Example();
            var record = new Record { Gemini = e };

            service.Upsert(record);

            string expectedWkt = BoundingBoxUtility.ToWkt(e.BoundingBox);
            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Wkt == expectedWkt)));
        }

        [Test]
        public void should_store_empty_bounding_box_as_null_wkt()
        {
            // to avoid raven / lucene indexing errors

            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub(), Mock.Of<IVocabularyService>());

            service.Upsert(BasicRecord());

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Wkt == null)));
        }

        [Test]
        public void should_always_set_resource_type_to_dataset()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub(), Mock.Of<IVocabularyService>());

            var record = BasicRecord().With(r => r.Gemini.ResourceType = "");
            service.Upsert(record);

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Gemini.ResourceType == "dataset")));
        }

        [Test]
        public void should_standardise_unconditional_use_constraints()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub(), Mock.Of<IVocabularyService>());

            var record = BasicRecord().With(r => r.Gemini.UseConstraints = "   No conditions APPLY");
            service.Upsert(record);

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Gemini.UseConstraints == "no conditions apply")));
        }

        [Test]
        public void should_set_security_to_open_by_default()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub(), Mock.Of<IVocabularyService>());

            service.Upsert(BasicRecord());

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Security == Security.Open)));
        }

        [Test]
        public void should_save_keywords_in_correct_order()
        {
            // should be sorted by vocab, then value, but with distinguished vocab "jncc broad category" first!
            // finally, keywords with no namespace should be last

            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub(), Mock.Of<IVocabularyService>());

            var record = BasicRecord().With(r =>
                {
                    r.Gemini.Keywords = new StringPairList
                        {
                            { "a-vocab-beginning-with-a", "bravo" },
                            { "boring-vocab-beginning-with-b", "some-keyword" },
                            { "a-vocab-beginning-with-a", "alpha" },
                            { "http://vocab.jncc.gov.uk/jncc-broad-category", "bravo" },
                            { "http://vocab.jncc.gov.uk/jncc-broad-category", "alpha" },
                            { "", "some-keyword" },
                        }.ToKeywordList();
                });

            service.Upsert(record);

            var expected = new StringPairList
                {
                    { "http://vocab.jncc.gov.uk/jncc-broad-category", "alpha" },
                    { "http://vocab.jncc.gov.uk/jncc-broad-category", "bravo" },
                    { "a-vocab-beginning-with-a", "alpha" },
                    { "a-vocab-beginning-with-a", "bravo" },
                    { "boring-vocab-beginning-with-b", "some-keyword" },
                    { "", "some-keyword" },
                }.ToKeywordList();

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Gemini.Keywords.IsEqualTo(expected))));
        }

        Record BasicRecord()
        {
            return new Record
                {
                    Path = @"X:\some\path",
                    Gemini = Library.Blank().With(m => m.Title = "Some title")
                };
        }

        /// <summary>
        /// A validator stub which returns a result with no validation errors.
        /// </summary>
        IRecordValidator ValidatorStub()
        {
            return Mock.Of<IRecordValidator>(v => v.Validate(It.IsAny<Record>()) == new RecordValidationResult());
        }

        /// <summary>
        /// A validator stub which returns a result containing a validation error.
        /// </summary>
        IRecordValidator FailingValidatorStub()
        {
            var r = new RecordValidationResult();
            r.Errors.Add("Don't be silly!");
            return Mock.Of<IRecordValidator>(v => v.Validate(It.IsAny<Record>()) == r);
        }
    }
}
