using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Web.Account;
using Catalogue.Web.Controllers.Records;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raven.Client;
using System;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Records
{
    public class records_controllers_tests : DatabaseTestFixture
    {
        [Test]
        public void should_return_blank_record_for_empty_guid()
        {
            var controller = new RecordsController(Mock.Of<IRecordService>(), Mock.Of<IDocumentSession>(), new TestUserContext());
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
            var controller = new RecordsController(service, Mock.Of<IDocumentSession>(), new TestUserContext());

            var result = controller.Post(record);

            result.Record.Id.Should().NotBeEmpty();
        }

        [Test]
        public void should_give_new_record_a_footer()
        {
            var record = new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m => m.Title = "Footer creation test")
            };
            var rsr = RecordServiceResult.SuccessfulResult.With(r => r.Record = record);
            var service = Mock.Of<IRecordService>(s => s.Insert(It.IsAny<Record>()) == rsr);
            var controller = new RecordsController(service, Mock.Of<IDocumentSession>(), new TestUserContext());

            var result = controller.Post(record);
            var footer = result.Record.Footer;
            footer.Should().NotBeNull();
            footer.CreatedOnUtc.Should().NotBe(DateTime.MinValue);
            footer.CreatedBy.Should().Be("Test User");
            footer.ModifiedOnUtc.Should().NotBe(DateTime.MinValue);
            footer.ModifiedBy.Should().Be("Test User");
        }

        [Test]
        public void should_update_footer_for_existing_record()
        {
            var recordId = new Guid("4d909f48-4547-4129-a663-bfab64ae97e9");
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m => m.Title = "Footer update test"),
                Footer = new Footer {
                    CreatedOnUtc = new DateTime(2015, 1, 1, 10, 0, 0),
                    CreatedBy = "Creator",
                    ModifiedOnUtc = new DateTime(2015, 1, 1, 10, 0, 0),
                    ModifiedBy = "Creator"
                }
            };
            Db.Store(record);

            var rsr = RecordServiceResult.SuccessfulResult.With(r => r.Record = record);
            var service = Mock.Of<IRecordService>(s => s.Update(It.IsAny<Record>()) == rsr);
            var controller = new RecordsController(service, Mock.Of<IDocumentSession>(), new TestUserContext());

            var result = controller.Put(recordId, record);
            var footer = result.Record.Footer;
            footer.Should().NotBeNull();
            footer.CreatedOnUtc.Should().Be(new DateTime(2015, 1, 1, 10, 0, 0));
            footer.CreatedBy.Should().Be("Creator");
            footer.ModifiedOnUtc.Should().Be(new DateTime(2015, 1, 1, 12, 0, 0));
            footer.ModifiedBy.Should().Be("Test User");
        }


    }
}
