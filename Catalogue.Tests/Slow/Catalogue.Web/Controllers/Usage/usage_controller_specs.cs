using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Time;
using Catalogue.Web.Controllers.Usage;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Usage
{
    public class usage_controller_specs
    {

        [Test]
        public void recently_modified_records()
        {
            var dateTest7 = new ModifiedRecord
            {
                Id = new Guid("1458dfd1-e356-4287-9190-65e5f9ffd1df"),
                Title = "DateTest_7",
                Date = new DateTime(2017, 07, 12, 15, 00, 00),
                User = "Cathy"
            };

            var dateTest5 = new ModifiedRecord
            {
                Id = new Guid("80de0c30-325a-4392-ab4e-64b0654ca6ec"),
                Title = "DateTest_5",
                Date = new DateTime(2017, 07, 11, 10, 00, 00),
                User = "Pete"
            };

            var dateTest4 = new ModifiedRecord
            {
                Id = new Guid("f5a48ac7-13f6-40ba-85a2-f4534d9806a5"),
                Title = "DateTest_4",
                Date = new DateTime(2017, 07, 01, 15, 05, 0),
                User = "Pete"
            };

            var dateTest3 = new ModifiedRecord
            {
                Id = new Guid("8c88dd97-3317-43e4-b59e-239e0604a094"),
                Title = "DateTest_3",
                Date = new DateTime(2017, 07, 01, 15, 00, 00),
                User = "Cathy"
            };

            var dateTest2 = new ModifiedRecord
            {
                Id = new Guid("3ad98517-110b-40d7-aa0d-f0e3b1273007"),
                Title = "DateTest_2",
                Date = new DateTime(2017, 06, 05, 09, 00, 00),
                User = "Pete"
            };

            var dateTest1 = new ModifiedRecord
            {
                Id = new Guid("8f4562ea-9d8a-45a0-afd3-bc5072d342a0"),
                Title = "DateTest_1",
                Date = new DateTime(2016, 01, 03, 09, 00, 00),
                User = "Cathy"
            };

            var testRecords = new List<ModifiedRecord> { dateTest7, dateTest5, dateTest4, dateTest3, dateTest2, dateTest1 };
            var expectedRecords = new List<ModifiedRecord> { dateTest7, dateTest5, dateTest4, dateTest3, dateTest2 };

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                IRecordService recordService = new RecordService(db, new RecordValidator());
                AddLastModifiedDateRecords(testRecords, recordService);
                db.SaveChanges();

                var usageController = new UsageController(db);

                var usageResult = usageController.GetRecentlyModifiedRecords();
                var recentlyModifiedRecords = usageResult.RecentlyModifiedRecords;


                recentlyModifiedRecords.Should().ContainInOrder(expectedRecords);
            }
        }

        private static void AddLastModifiedDateRecords(List<ModifiedRecord> testRecords, IRecordService recordService)
        {
            foreach (var testRecord in testRecords)
            {
                Clock.CurrentUtcDateTimeGetter = () => testRecord.Date;

                var record = new Record().With(r =>
                {
                    r.Id = testRecord.Id;
                    r.Path = @"X:\path\to\last\modified\date\data\" + testRecord.Title;
                    r.Gemini = Library.Blank().With(m =>
                    {
                        m.Title = testRecord.Title;
                        m.Abstract = "Record with different last modified date";
                        m.MetadataPointOfContact.Name = testRecord.User;
                        m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine" });
                        m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Overseas Territories" });
                        m.ResourceType = "dataset";
                    });
                });

                recordService.Insert(record);
            }
        }
    }
}