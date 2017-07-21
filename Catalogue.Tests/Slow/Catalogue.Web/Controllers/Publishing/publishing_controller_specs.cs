using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using System;
using Catalogue.Gemini.Model;
using Catalogue.Web.Account;
using Catalogue.Web.Controllers.Publishing;
using Moq;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Publishing
{
    class publishing_controller_specs
    {
        [Test]
        public void open_data_sign_off_test()
        {
            var recordId = new Guid("f34de2d3-17af-47e2-8deb-a16b67c76b06");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data sign off test";
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Terrestrial" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Example Collection" });
                });
            });

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var user = new User("Guest User", "Guest", "guest@example.com", "none!");
                var userContextMock = new Mock<IUserContext>();
                userContextMock.Setup(u => u.User).Returns(user);

                var recordService = new RecordService(db, new RecordValidator());
                var publishingService = new OpenDataPublishingService(db, recordService);
                var publishingController = new OpenDataPublishingController(db, publishingService, userContextMock.Object);
                var request = new SignOffRequest
                {
                    Id = recordId,
                    Comment = "Sign off test"
                };
                publishingController.SignOff(request);

                var markedRecord = db.Load<Record>(recordId);
                markedRecord.Publication.Should().NotBeNull();

                var openDataInfo = markedRecord.Publication.OpenData;
                openDataInfo.Should().NotBeNull();
                openDataInfo.LastAttempt.Should().BeNull();
                openDataInfo.LastSuccess.Should().BeNull();
                openDataInfo.Resources.Should().BeNull();
                openDataInfo.Paused.Should().BeFalse();
                openDataInfo.SignOff.User.Should().Be("Guest User");
                openDataInfo.SignOff.DateUtc.Should().NotBe(DateTime.MinValue);
                openDataInfo.SignOff.Comment.Should().Be("Sign off test");
            }
        }
    }
}
