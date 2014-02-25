using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Spatial;
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
        readonly IDocumentSession db;
        readonly IRecordValidator validator;

        public RecordService(IDocumentSession db, IRecordValidator validator)
        {
            this.db = db;
            this.validator = validator;
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
            this.SyncDenormalizations(record);

            // currently only supporting dataset resource types
            record.Gemini.ResourceType = "dataset";

            NormalizeUseConstraints(record);

            var errors = validator.Validate(record);

            if (!errors.Any())
                db.Store(record);

            return new RecordServiceResult { Errors = errors };
        }

        void NormalizeUseConstraints(Record record)
        {
            const string none = "no conditions apply";

            if (record.Gemini.UseConstraints.IsNotBlank() && record.Gemini.UseConstraints.ToLowerInvariant().Trim() == none)
                record.Gemini.UseConstraints = none;
        }

        void SyncDenormalizations(Record record)
        {
            // we store the bounding box as wkt so we can index it
            if (!BoundingBoxUtility.IsBlank(record.Gemini.BoundingBox))
                record.Wkt = BoundingBoxUtility.ToWkt(record.Gemini.BoundingBox);
        }
    }

    public class RecordServiceResult
    {
        public RecordValidationErrorSet Errors { get; set; }
        public bool Success { get { return Errors == null || !Errors.Any(); } }
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
            var service = new RecordService(Mock.Of<IDocumentSession>(), Mock.Of<IRecordValidator>());
            var record = new Record { ReadOnly = true };

            service.Invoking(s => s.Update(record))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Cannot update a read-only record.");
        }
    }

    class when_upserting_a_record
    {
        Record BlankRecord()
        {
            return new Record { Path = @"X:\some\path", Gemini = Library.Blank() };
        }

        [Test]
        public void should_store_record_in_the_database()
        {
            // todo: 
            
        }
        
        [Test]
        public void bounding_box_should_be_stored_as_wkt()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, GetValidatorStub());
            
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
            var service = new RecordService(database, GetValidatorStub());

            var record = BlankRecord();
            service.Upsert(record);

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Wkt == null)));
        }

        [Test]
        public void should_always_set_resource_type_to_dataset()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, GetValidatorStub());

            var record = BlankRecord().With(r => r.Gemini.ResourceType = "");
            service.Upsert(record);

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Gemini.ResourceType == "dataset")));
        }

        [Test]
        public void should_normalise_use_constraints()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, GetValidatorStub());

            var record = BlankRecord().With(r => r.Gemini.UseConstraints = "   No conditions APPLY");
            service.Upsert(record);

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Gemini.UseConstraints == "no conditions apply")));
        }

        [Test]
        public void should_set_security_to_open_by_default()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, GetValidatorStub());

            var record = BlankRecord();
            service.Upsert(record);

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Security == Security.Open)));
        }

        // todo should save keywords in correct order - 
        // first http://vocab.jncc.gov.uk/jncc-broad-category
        // then sort by vocab, then value

        IRecordValidator GetValidatorStub()
        {
            return Mock.Of<IRecordValidator>(v => v.Validate(It.IsAny<Record>()) == new RecordValidationErrorSet());
        }
    }
}
