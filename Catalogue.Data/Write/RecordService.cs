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
        RecordInsertionResult Insert(Record record);
    }

    public class RecordService : IRecordService
    {
        readonly IDocumentSession db;

        public RecordService(IDocumentSession db)
        {
            this.db = db;
        }

        public Record Load(Guid id)
        {
            return db.Load<Record>(id);
        }

        public RecordInsertionResult Insert(Record record)
        {
            if (record.Gemini.ResourceLocator.IsBlank())
                return new RecordInsertionResult { Message = "ResourceLocator must not be blank." };

            db.Store(record);

            return new RecordInsertionResult { Succeeded = true };
        }


    }

    public class RecordInsertionResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
    }


    class when_inserting_a_record_with_blank_resource_locator
    {
        [Test]
        public void should_not_succeed()
        {
            var record = new Record
                {
                    Gemini = new Metadata
                        {
                            Title = "A record without a ResourceLocator",
                            ResourceLocator = null,
                        }
                };

            var service = new RecordService(Mock.Of<IDocumentSession>());

            var result = service.Insert(record);
            result.Succeeded.Should().BeFalse();
            result.Message.Should().StartWith("ResourceLocator must not be blank");
        }
    }
}
