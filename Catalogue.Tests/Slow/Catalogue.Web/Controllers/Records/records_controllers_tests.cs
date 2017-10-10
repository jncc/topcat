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
    public class records_controllers_tests
    {
        [Test]
        public void should_return_blank_record_for_empty_guid()
        {
            var controller = new RecordsController(Mock.Of<IUserRecordService>(), Mock.Of<IDocumentSession>(), new TestUserContext());
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
            var service = Mock.Of<IUserRecordService>(s => s.Insert(It.IsAny<Record>(), It.IsAny<UserInfo>()) == rsr);
            var controller = new RecordsController(service, Mock.Of<IDocumentSession>(), new TestUserContext());

            var result = controller.Post(record);

            result.Record.Id.Should().NotBeEmpty();
        }

        [Test]
        public void cloned_record_should_have_null_publication()
        {
            var record = new Record
            {
                Id = new Guid("736532c8-9b3d-4524-86ac-248e0476fa38"),
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m => m.Title = "Some new record!"),
                Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true
                        }
                    }
                }
            };

            var db = Mock.Of<IDocumentSession>(d => d.Load<Record>(It.IsAny<Guid>()) == record);
            var service = Mock.Of<IUserRecordService>();
            var controller = new RecordsController(service, db, new TestUserContext());

            var result = controller.Get(record.Id, true);
            result.Id.Should().BeEmpty();
            result.Path.Should().BeEmpty();
            result.Gemini.Title.Should().BeEmpty();
            result.Publication.Should().BeNull();
        }
    }
}
