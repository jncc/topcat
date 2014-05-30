using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Web.Controllers.Records
{
    public class RecordsController : ApiController
    {
        readonly IDocumentSession db;
        readonly IRecordService service;

        public RecordsController(IRecordService service, IDocumentSession db)
        {
            this.service = service;
            this.db = db;
        }

        // GET api/records/57d34691-9064-4c1e-90a7-7b0c112daa8d (get a record)

        public Record Get(Guid id)
        {
            if (id == Guid.Empty) // a nice empty record for making a new one
            {
                return new Record
                    {
                        Id = Guid.Empty,
                        Gemini = Library.Blank(),
                        Review = DateTime.Now
                    };
            }
            else
            {
                return service.Load(id);
            }
        }

        // PUT api/records/57d34691-9064-4c1e-90a7-7b0c112daa8d (update/replace a record)

        public RecordServiceResult Put(Guid id, [FromBody]Record record)
        {
            // todo should check ID in record is the same as being PUT to

            var result = service.Update(record);

            if (result.Success)
                db.SaveChanges();

            return result;
        }

        public RecordServiceResult Post([FromBody] Record record)
        {
            record.Id = Guid.NewGuid();

            var result = service.Insert(record);

            if (result.Success)
                db.SaveChanges();

            return result;
        }

    }

    public class records_controllers_tests
    {
        [Test]
        public void should_return_blank_record_for_empty_guid()
        {
            var controller = new RecordsController(Mock.Of<IRecordService>(), Mock.Of<IDocumentSession>());
            var record = controller.Get(Guid.Empty);

            record.Gemini.Title.Should().BeBlank();
            record.Path.Should().BeBlank();
        }

        [Test]
        public void should_give_new_record_a_new_guid()
        {
            var record = new Record
                {
                    Path = @"X:\some\path",
                    Gemini = Library.Blank().With(m => m.Title = "Some new record!")
                };
            var rsr = RecordServiceResult.SuccessfulResult.With(r => r.Record = record);
            var service = Mock.Of<IRecordService>(s => s.Insert(It.IsAny<Record>()) == rsr);
            var controller = new RecordsController(service, Mock.Of<IDocumentSession>());
            
            var result = controller.Post(record);

            result.Record.Id.Should().NotBeEmpty();
        }
    }
}

