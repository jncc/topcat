using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Web.Controllers.Publishing;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Publishing
{
    class publishing_controller_specs
    {
        [Test]
        public void mark_as_open_data_test()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("f34de2d3-17af-47e2-8deb-a16b67c76b06");
                r.Path = @"X:\path\to\marking\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data marking test";
                });
            });

            record.Publication.Should().BeNull();

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var recordService = new RecordService(db, new RecordValidator());
                var publishingService = new OpenDataPublishingService(db, recordService);
                var publishingController = new OpenDataPublishingController(db, publishingService);
                publishingController.MarkAsOpenData(record.Id);

                var markedRecord = db.Load<Record>(record.Id);
                markedRecord.Publication.Should().NotBeNull();
                markedRecord.Publication.OpenData.Should().NotBeNull();
                markedRecord.Publication.OpenData.LastAttempt.Should().BeNull();
                markedRecord.Publication.OpenData.LastSuccess.Should().BeNull();
                markedRecord.Publication.OpenData.Resources.Should().BeNull();
                markedRecord.Publication.OpenData.Paused.Should().BeFalse();
            }
        }
    }
}
