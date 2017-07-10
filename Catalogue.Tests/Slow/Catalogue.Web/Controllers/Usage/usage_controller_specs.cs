using Catalogue.Web.Controllers.Usage;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Time;
using Raven.Client;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Usage
{
    public class usage_controller_specs
    {

        [Test]
        public void recently_modified_records()
        {
            var dateTest7 = new ModifiedRecord();
            dateTest7.Id = new Guid("1458dfd1-e356-4287-9190-65e5f9ffd1df");
            dateTest7.Title = "DateTest_7";
            dateTest7.Date = new DateTime(2017, 07, 12, 15, 00, 00);
            dateTest7.User = "Cathy";

            var dateTest5 = new ModifiedRecord();
            dateTest5.Id = new Guid("80de0c30-325a-4392-ab4e-64b0654ca6ec");
            dateTest5.Title = "DateTest_5";
            dateTest5.Date = new DateTime(2017, 07, 11, 10, 00, 00);
            dateTest5.User = "Pete";

            var dateTest4 = new ModifiedRecord();
            dateTest4.Id = new Guid("f5a48ac7-13f6-40ba-85a2-f4534d9806a5");
            dateTest4.Title = "DateTest_4";
            dateTest4.Date = new DateTime(2017, 07, 01, 15, 05, 0);
            dateTest4.User = "Pete";

            var dateTest3 = new ModifiedRecord();
            dateTest3.Id = new Guid("8c88dd97-3317-43e4-b59e-239e0604a094");
            dateTest3.Title = "DateTest_3";
            dateTest3.Date = new DateTime(2017, 07, 01, 15, 00, 00);
            dateTest3.User = "Cathy";

            var dateTest2 = new ModifiedRecord();
            dateTest2.Id = new Guid("3ad98517-110b-40d7-aa0d-f0e3b1273007");
            dateTest2.Title = "DateTest_2";
            dateTest2.Date = new DateTime(2017, 06, 05, 09, 00, 00);
            dateTest2.User = "Pete";

            var expectedList = new List<ModifiedRecord>();
            expectedList.Add(dateTest7);
            expectedList.Add(dateTest5);
            expectedList.Add(dateTest4);
            expectedList.Add(dateTest3);
            expectedList.Add(dateTest2);

            IDocumentStore store = new InMemoryDatabaseHelper().Create();
            using (IDocumentSession db = store.OpenSession())
            {
                IRecordService recordService = new RecordService(db, new RecordValidator());
                AddLastModifiedDateRecords(recordService);
                db.SaveChanges();

                var usageController = new UsageController(db);

                var usageResult = usageController.GetRecentlyModifiedRecords();
                var recentlyModifiedRecords = usageResult.RecentlyModifiedRecords;


                recentlyModifiedRecords.Should().ContainInOrder(expectedList);
            }
        }

        void AddLastModifiedDateRecords(IRecordService recordService)
        {
            var tuples = new[]
            {
                Tuple.Create("8f4562ea-9d8a-45a0-afd3-bc5072d342a0", "DateTest_1",
                    new DateTime(2016, 01, 03, 09, 00, 00), "Cathy"),
                Tuple.Create("3ad98517-110b-40d7-aa0d-f0e3b1273007", "DateTest_2",
                    new DateTime(2017, 06, 05, 09, 00, 00), "Pete"),
                Tuple.Create("8c88dd97-3317-43e4-b59e-239e0604a094", "DateTest_3",
                    new DateTime(2017, 07, 01, 15, 00, 00), "Cathy"),
                Tuple.Create("f5a48ac7-13f6-40ba-85a2-f4534d9806a5", "DateTest_4",
                    new DateTime(2017, 07, 01, 15, 05, 00), "Pete"),
                Tuple.Create("80de0c30-325a-4392-ab4e-64b0654ca6ec", "DateTest_5",
                    new DateTime(2017, 07, 11, 10, 00, 00), "Pete"),
                Tuple.Create("1458dfd1-e356-4287-9190-65e5f9ffd1df", "DateTest_7",
                    new DateTime(2017, 07, 12, 15, 00, 00), "Cathy"),
            };

            foreach (Tuple<string, string, DateTime, string> tuple in tuples)
            {
                Clock.CurrentUtcDateTimeGetter = () => tuple.Item3;

                var record = MakeExampleSeedRecord().With(r =>
                {
                    r.Id = new Guid(tuple.Item1);
                    r.Path = @"X:\path\to\last\modified\date\data\" + tuple.Item2;
                    r.Gemini = r.Gemini.With(m =>
                    {
                        m.Title = tuple.Item2;
                        m.Abstract = "Record with different last modified date";
                        m.MetadataPointOfContact.Name = tuple.Item4;
                    });
                });

                recordService.Insert(record);
            }
        }

        Record MakeExampleSeedRecord()
        {
            return new Record
            {
                Gemini = Library.Blank().With(m =>
                {
                    m.ResourceType = "dataset";
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Terrestrial" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Example Collection" });
                    m.Keywords.Add(new MetadataKeyword { Vocab = "", Value = "example" });
                }),
            };
        }
    }
}