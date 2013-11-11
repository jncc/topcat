using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
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
        RecordValidationResult Insert(Record record);
        RecordValidationResult Update(Record record);
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

        public RecordValidationResult Insert(Record record)
        {
            return Upsert(record);
        }

        public RecordValidationResult Update(Record record)
        {
            // additional validation for updating
            if (record.ReadOnly)
                return new RecordValidationResult { Message = "Cannot update read-only record." };

            return Upsert(record);
        }

        RecordValidationResult Upsert(Record record)
        {
            var result = validator.Validate(record);

            if (result.Success)
                db.Store(record);

            return result;
        }
    }


    // this is really a test of the validator, so needs moving
    class when_inserting_a_record
    {
        [Test]
        public void should_fail_if_path_is_blank()
        {
            var record = new Record
                {
                    Path = null,
                    Gemini = new Metadata { Title = "A record without a path" }
                };

            var service = new RecordService(Mock.Of<IDocumentSession>(), new RecordValidator());

            var result = service.Insert(record);
            result.Success.Should().BeFalse();
            result.Message.Should().StartWith("Path must not be blank");
        }
    }

    class when_updating_a_record
    {
        [Test]
        public void should_fail_if_record_is_readonly()
        {
            var service = new RecordService(Mock.Of<IDocumentSession>(), new RecordValidator());

            var record = new Record { ReadOnly = true };

            var result = service.Update(record);
            result.Success.Should().BeFalse();
            result.Message.Should().StartWith("Cannot update read-only record");
        }
    }
}
