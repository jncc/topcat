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
using Catalogue.Data;
using Raven.Client.Documents.Session;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Records
{
    public class records_controllers_tests
    {
        [Test]
        public void should_return_blank_record_for_empty_guid()
        {
            var controller = new RecordsController(Mock.Of<IRecordService>(), Mock.Of<IDocumentSession>(), new TestUserContext());
            var recordResult = (RecordOutputModel) controller.Get(String.Empty);

            recordResult.Record.Gemini.Title.Should().Be("");
            recordResult.Record.Path.Should().BeNull();
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
            var service = Mock.Of<IRecordService>(s => s.Insert(It.IsAny<Record>(), It.IsAny<UserInfo>()) == rsr);
            var controller = new RecordsController(service, Mock.Of<IDocumentSession>(), new TestUserContext());

            var result = (RecordServiceResult) controller.Post(record);

            result.Record.Id.Should().NotBeEmpty();
        }

        [Test]
        public void cloned_record_should_have_null_publication()
        {
            var record = new Record
            {
                Id = Helpers.AddCollection("736532c8-9b3d-4524-86ac-248e0476fa38"),
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m => m.Title = "Some new record!"),
                Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true
                    }
                }
            };

            var db = Mock.Of<IDocumentSession>(d => d.Load<Record>(It.IsAny<String>()) == record);
            var service = Mock.Of<IRecordService>();
            var controller = new RecordsController(service, db, new TestUserContext());

            var result = (RecordOutputModel) controller.Get(Helpers.RemoveCollection(record.Id), true);
            result.Record.Id.Should().Be(Guid.Empty.ToString());
            result.Record.Path.Should().BeEmpty();
            result.Record.Gemini.Title.Should().BeEmpty();
            result.Record.Publication.Should().BeNull();
        }
    }
}
