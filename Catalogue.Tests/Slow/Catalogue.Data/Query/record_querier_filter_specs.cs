using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents.Session;
using System;
using System.Linq;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Tests.Slow.Catalogue.Data.Query
{
    public class record_querier_filter_specs : CleanDbTest
    {
        protected IDocumentSession Db;

        [SetUp]
        public new void SetUp()
        {
            var store = GetDocumentStore();
            store.Initialize();
            IndexCreation.CreateIndexes(typeof(Record).Assembly, store);
            WaitForIndexing(store);
            ReusableDocumentStore = store;

            Db = ReusableDocumentStore.OpenSession();
            
            var record1 = QueryTestHelper.SimpleRecord().With(m =>
            {
                m.Gemini.Title = "spreadsheet record";
                m.Gemini.DataFormat = "Microsoft Excel for Windows";
                m.Gemini.ResourceType = "publication";
                m.Manager = new UserInfo { DisplayName = "cathy test cathy.test@jncc.gov.uk" };
            });
            var record2 = QueryTestHelper.SimpleRecord().With(m =>
            {
                m.Gemini.Title = "database record";
                m.Gemini.DataFormat = "Database";
                m.Gemini.ResourceType = "nonGeographicDataset";
                m.Manager = new UserInfo { DisplayName = "pete test pete.test@jncc.gov.uk" };
            });
            var record3 = QueryTestHelper.SimpleRecord().With(m =>
            {
                m.Gemini.Title = "geospatial record 1";
                m.Gemini.DataFormat = "ESRI Arc/View ShapeFile";
                m.Gemini.ResourceType = "dataset";
                m.Manager = new UserInfo { DisplayName = "pete test" };
            });
            var record4 = QueryTestHelper.SimpleRecord().With(m =>
            {
                m.Gemini.Title = "geospatial record 2";
                m.Gemini.DataFormat = "Geospatial (vector polygon)";
                m.Gemini.ResourceType = "service";
                m.Manager = new UserInfo { DisplayName = "cathy.test@jncc.gov.uk" };
            });
            var record5 = QueryTestHelper.SimpleRecord().With(m =>
            {
                m.Gemini.Title = "record with no data format";
                m.Gemini.DataFormat = null;
                m.Gemini.ResourceType = "dataset";
            });

            Db.Store(record1);
            Db.Store(record2);
            Db.Store(record3);
            Db.Store(record4);
            Db.Store(record5);
            Db.SaveChanges();
            WaitForIndexing(ReusableDocumentStore);
        }

        [TearDown]
        public new void TearDown()
        {
            Db.Dispose();
            ReusableDocumentStore.Dispose();
            Dispose();
        }

        [Test]
        public void blank_keyword_search_test()
        {
            var helper = new RecordQueryer(Db);

            var input = new RecordQueryInputModel
            {
                Q = "",
                F = new FilterOptions { Keywords = new[] { "" } },
                P = 0,
                N = 25,
                O = 0
            };

            helper.Search(input).Results.Count.Should().Be(5);
        }

        [Test]
        public void search_by_metadata_date()
        {
            //Save some data
            using (var db = ReusableDocumentStore.OpenSession())
            {
                var record = new Record
                {
                    Path = @"X:\some\path",
                    Gemini = Library.Blank().With(m =>
                    {
                        m.Title = "Some title";
                        m.MetadataDate = DateTime.Parse("2020-01-01");
                    }),
                    Footer = new Footer()
                };

                db.Store(record);
                db.SaveChanges();
                WaitForIndexing(ReusableDocumentStore);

                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "",
                    F = new FilterOptions { MetadataDate = DateTime.Parse("2020-01-01") },
                    P = 0,
                    N = 25,
                    O = 0
                };

                helper.Query(input).Count().Should().Be(1);
            }
        }

        [Test]
        public void filter_by_format_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions {DataFormats = new[] { "Geospatial" }};
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "geospatial <b>record</b> 1").Should().BeTrue();
            results.Any(r => r.Title == "geospatial <b>record</b> 2").Should().BeTrue();
        }

        [Test]
        public void filter_by_format_test_with_no_query()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "";
                x.F = new FilterOptions { DataFormats = new[] { "Geospatial" } };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "geospatial record 1").Should().BeTrue();
            results.Any(r => r.Title == "geospatial record 2").Should().BeTrue();
        }

        [Test]
        public void filter_by_multiple_formats_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions {DataFormats = new[] { "Spreadsheet", "Database" }};
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
            results.Any(r => r.Title == "database <b>record</b>").Should().BeTrue();
        }

        [Test]
        public void filter_by_no_formats_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x => x.F = new FilterOptions { DataFormats = new string[0]});

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(5);
        }

        [Test]
        public void filter_by_other_formats_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions { DataFormats = new[] { "Other" } };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(1);
            results.Any(r => r.Title == "<b>record</b> with no data format").Should().BeTrue();
        }

        [Test]
        public void filter_by_multiple_formats_including_other_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions { DataFormats = new[] { "Other", "Database" } };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "<b>record</b> with no data format").Should().BeTrue();
            results.Any(r => r.Title == "database <b>record</b>").Should().BeTrue();
        }

        [Test]
        public void filter_user_with_partial_user_name_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions { Manager = "CATHY" };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
            results.Any(r => r.Title == "geospatial <b>record</b> 2").Should().BeTrue();
        }

        [Test]
        public void filter_user_with_full_user_name_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions { Manager = "cathy TEST" };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
            results.Any(r => r.Title == "geospatial <b>record</b> 2").Should().BeTrue();
        }

        [Test]
        public void filter_user_with_email_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions { Manager = "cathy.test@jncc.gov.uk" };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
            results.Any(r => r.Title == "geospatial <b>record</b> 2").Should().BeTrue();
        }

        [Test]
        public void filter_user_with_name_and_email_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions { Manager = "cathy test" };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
            results.Any(r => r.Title == "geospatial <b>record</b> 2").Should().BeTrue();
        }

        [Test]
        public void filter_by_email_test()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions { Manager = "jncc.gov.uk" };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(3);
        }

        [Test]
        public void filter_by_one_resource_type()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions { ResourceTypes = new [] {"Dataset"} };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "geospatial <b>record</b> 1").Should().BeTrue();
            results.Any(r => r.Title == "<b>record</b> with no data format").Should().BeTrue();
        }

        [Test]
        public void filter_by_multiple_resource_types()
        {
            var queryer = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x =>
            {
                x.Q = "record";
                x.F = new FilterOptions { ResourceTypes = new[] { "publication", "nonGeographicDataset" } };
            });

            var results = queryer.Search(input).Results;
            results.Count.Should().Be(2);
            results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
            results.Any(r => r.Title == "database <b>record</b>").Should().BeTrue();
        }
    }
}
