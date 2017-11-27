using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Test;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using System;
using System.Linq;

namespace Catalogue.Tests.Slow.Catalogue.Data.Query
{
    class record_querier_filter_specs : DatabaseTestFixture
    {
        object[] KeywordTestCases =
        {
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction ", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction (abstraction)", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction – Water (abstraction)", 1 }, // note unicode dash!!
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction – Water", 0 },
            new object[] { "vocab.jncc.gov.uk/human-activity/Extraction", 0 },
            new object[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps", 189 },
            new object[] { "vocab.jncc.gov.uk/some-vocab/Two words", 1 },
        };

        [Test, TestCaseSource("KeywordTestCases")]
        public void should_return_correct_result_count_for_keywords(string keyword, int expected)
        {
            var input = QueryTestHelper.EmptySearchInput().With(q => q.F = new FilterOptions { Keywords = new[] { keyword } });
            var output = new RecordQueryer(Db).Search(input);

            output.Total.Should().Be(expected);
        }

        [Test]
        public void keyword_search_test()
        {
            var helper = new RecordQueryer(Db);

            var input = new RecordQueryInputModel
            {
                Q = "",
                F = new FilterOptions { Keywords = new[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" } },
                P = 0,
                N = 25,
                O = 0
            };

            helper.Search(input).Results.Count().Should().Be(25);
        }

        [Test]
        public void blank_keyword_search_test()
        {
            var helper = new RecordQueryer(GetDbForFilterTests());

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
            using (var db1 = ReusableDocumentStore.OpenSession())
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

                Db.Store(record);
                Db.SaveChanges();
            }

            using (var db2 = ReusableDocumentStore.OpenSession())
            {
                var helper = new RecordQueryer(db2);

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
            using(var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
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
        }

        [Test]
        public void filter_by_format_test_with_no_query()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
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
        }

        [Test]
        public void filter_by_multiple_formats_test()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
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
        }

        [Test]
        public void filter_by_no_formats_test()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
                var input = QueryTestHelper.EmptySearchInput().With(x => x.F = new FilterOptions { DataFormats = new string[0]});

                var results = queryer.Search(input).Results;
                results.Count.Should().Be(5);
            }
        }

        [Test]
        public void filter_by_other_formats_test()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
                var input = QueryTestHelper.EmptySearchInput().With(x =>
                {
                    x.Q = "record";
                    x.F = new FilterOptions { DataFormats = new[] { "Other" } };
                });

                var results = queryer.Search(input).Results;
                results.Count.Should().Be(1);
                results.Any(r => r.Title == "<b>record</b> with no data format").Should().BeTrue();
            }
        }

        [Test]
        public void filter_by_multiple_formats_including_other_test()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
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
        }

        [Test]
        public void filter_user_with_partial_user_name_test()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
                var input = QueryTestHelper.EmptySearchInput().With(x =>
                {
                    x.Q = "record";
                    x.F = new FilterOptions { User = new UserInfo{ DisplayName = "CATHY" } };
                });

                var results = queryer.Search(input).Results;
                results.Count.Should().Be(1);
                results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
            }
        }

        [Test]
        public void filter_user_with_full_user_name_test()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
                var input = QueryTestHelper.EmptySearchInput().With(x =>
                {
                    x.Q = "record";
                    x.F = new FilterOptions { User = new UserInfo { DisplayName = "cathy TEST" } };
                });

                var results = queryer.Search(input).Results;
                results.Count.Should().Be(1);
                results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
            }
        }

        [Test]
        public void filter_user_with_email_test()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
                var input = QueryTestHelper.EmptySearchInput().With(x =>
                {
                    x.Q = "record";
                    x.F = new FilterOptions { User = new UserInfo { Email = "cathy.test@jncc.gov.uk" } };
                });

                var results = queryer.Search(input).Results;
                results.Count.Should().Be(2);
                results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
                results.Any(r => r.Title == "geospatial <b>record</b> 2").Should().BeTrue();
            }
        }

        [Test]
        public void filter_user_with_name_and_email_test()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
                var input = QueryTestHelper.EmptySearchInput().With(x =>
                {
                    x.Q = "record";
                    x.F = new FilterOptions { User = new UserInfo { DisplayName = "cathy test", Email = "cathy test" } };
                });

                var results = queryer.Search(input).Results;
                results.Count.Should().Be(1);
                results.Any(r => r.Title == "spreadsheet <b>record</b>").Should().BeTrue();
            }
        }

        [Test]
        public void filter_by_email_test()
        {
            using (var db = GetDbForFilterTests())
            {
                var queryer = new RecordQueryer(db);
                var input = QueryTestHelper.EmptySearchInput().With(x =>
                {
                    x.Q = "record";
                    x.F = new FilterOptions { User = new UserInfo { Email = "test" } };
                });

                var results = queryer.Search(input).Results;
                results.Count.Should().Be(3);
            }
        }

        private IDocumentSession GetDbForFilterTests()
        {
            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                var record1 = QueryTestHelper.SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "spreadsheet record";
                    m.Gemini.DataFormat = "Microsoft Excel for Windows";
                    m.Manager = new UserInfo {DisplayName = "cathy test", Email = "cathy.test@jncc.gov.uk"};
                });
                var record2 = QueryTestHelper.SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "database record";
                    m.Gemini.DataFormat = "Database";
                    m.Manager = new UserInfo { DisplayName = "pete test", Email = "pete.test@jncc.gov.uk" };
                });
                var record3 = QueryTestHelper.SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "geospatial record 1";
                    m.Gemini.DataFormat = "ESRI Arc/View ShapeFile";
                    m.Manager = new UserInfo { DisplayName = "pete test"};
                });
                var record4 = QueryTestHelper.SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "geospatial record 2";
                    m.Gemini.DataFormat = "Geospatial (vector polygon)";
                    m.Manager = new UserInfo { Email = "cathy.test@jncc.gov.uk" };
                });
                var record5 = QueryTestHelper.SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "record with no data format";
                    m.Gemini.DataFormat = null;
                });

                db.Store(record1);
                db.Store(record2);
                db.Store(record3);
                db.Store(record4);
                db.Store(record5);
                db.SaveChanges();

                return db;
            }
        }
    }
}
