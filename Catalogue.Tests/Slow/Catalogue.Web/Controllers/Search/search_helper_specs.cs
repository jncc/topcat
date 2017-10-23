using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Test;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using Catalogue.Web.Controllers;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using Raven.Database.Indexing.Collation;
using static Catalogue.Data.Query.RecordQueryInputModel.SortOptions;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Search
{
    class record_querier_specs : DatabaseTestFixture
    {
        private Record SimpleRecord()
        {
            return new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Some title";
                    m.Keywords = new StringPairList
                            {
                                { "http://vocab.jncc.gov.uk/jncc-domain", "Example Domain" },
                                { "http://vocab.jncc.gov.uk/jncc-category", "Example Category" },
                            }
                        .ToKeywordList();
                }),
            };
        }

        [Test]
        public void keyword_search_test()
        {
            var helper = new RecordQueryer(Db);

            var input = new RecordQueryInputModel
                {
                    Q = "",
                    K = new [] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" },
                    P = 0,
                    N = 25,
                    D = null,
                    O = 0
                };

            helper.Search(input).Results.Count().Should().Be(25);
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
                    K = null,
                    P = 0,
                    N = 25,
                    D = DateTime.Parse("2020-01-01"),
                    O = 0
                };

                helper.Query(input).Count().Should().Be(1);

            }
        }

        [Test]
        public void test_results_sorted_by_relevance()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "birds",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("<b>birds</b>");
                results.ToList()[1].Title.Should().Be("coastal <b>birds</b>");
                results.ToList()[2].Title.Should().Be("sea<b>birds</b>");
            }
        }

        [Test]
        public void test_results_sorted_by_title_desc()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = TitleAZ
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(4);
                results.ToList()[0].Title.Should().Be("birds");
                results.ToList()[1].Title.Should().Be("coastal birds");
                results.ToList()[2].Title.Should().Be("sea");
                results.ToList()[3].Title.Should().Be("seabirds");
            }
        }

        [Test]
        public void test_results_sorted_by_title_asc()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = TitleZA
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(4);
                results.ToList()[0].Title.Should().Be("seabirds");
                results.ToList()[1].Title.Should().Be("sea");
                results.ToList()[2].Title.Should().Be("coastal birds");
                results.ToList()[3].Title.Should().Be("birds");
            }
        }

        [Test]
        public void test_results_sorted_by_dataset_reference_date_desc()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = NewestToOldest
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(4);
                results.ToList()[0].Title.Should().Be("seabirds");
                results.ToList()[1].Title.Should().Be("coastal birds");
                results.ToList()[2].Title.Should().Be("birds");
                results.ToList()[3].Title.Should().Be("sea");
            }
        }

        [Test]
        public void test_results_sorted_by_dataset_reference_date_asc()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = OldestToNewest
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(4);
                results.ToList()[0].Title.Should().Be("sea");
                results.ToList()[1].Title.Should().Be("birds");
                results.ToList()[2].Title.Should().Be("coastal birds");
                results.ToList()[3].Title.Should().Be("seabirds");
            }
        }

        [Test]
        public void test_results_sorted_by_title_works_with_bolding()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var record = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "cowsea";
                    m.Gemini.DatasetReferenceDate = "2017-10-11";
                });

                db.Store(record);
                db.SaveChanges();

                Thread.Sleep(100);

                var input = new RecordQueryInputModel
                {
                    Q = "sea",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = TitleAZ
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("cow<b>sea</b>");
                results.ToList()[1].Title.Should().Be("<b>sea</b>");
                results.ToList()[2].Title.Should().Be("<b>sea</b>birds");

                input = new RecordQueryInputModel
                {
                    Q = "sea",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = TitleZA
                };

                results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("<b>sea</b>birds");
                results.ToList()[1].Title.Should().Be("<b>sea</b>");
                results.ToList()[2].Title.Should().Be("cow<b>sea</b>");
            }
        }

        [Test]
        public void test_exact_search_query()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""sea""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>sea</b>");
            }
        }

        [Test]
        public void test_exact_search_query_works_with_sorting()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""birds""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = TitleAZ
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>birds</b>");
            }
        }

        [Test]
        public void test_exact_search_with_multiple_terms()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""coastal""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(0);

                input = new RecordQueryInputModel
                {
                    Q = @"""coastal birds""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>coastal birds</b>");
            }
        }

        [Test]
        public void test_exact_search_is_case_insensitive()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""BIRDS""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = TitleAZ
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>birds</b>");
            }
        }

        [Test]
        public void test_multiple_exact_search_terms()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""birds"" ""coastal""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>coastal</b> <b>birds</b>");
            }
        }

        [Test]
        public void test_exact_search_term_with_normal_search_term()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""birds"" coast",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>coast</b>al <b>birds</b>");
            }
        }

        [Test]
        public void test_exact_search_with_numbers_in_query()
        {
            var db = GetDbForSearchTestsWithDigits();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""ABCD1234""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>ABCD1234</b>");
            }
        }

        [Test]
        public void test_number_search()
        {
            var db = GetDbForSearchTestsWithDigits();
            using (db)
            {
                var helper = new RecordQueryer(db);
                var input = new RecordQueryInputModel
                {
                    Q = "1234",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };
                var results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("<b>1234</b>");
                results.ToList()[1].Title.Should().Be("ABCD <b>1234</b>");
                results.ToList()[2].Title.Should().Be("ABCD<b>1234</b>");
            }
        }

        [Test]
        public void test_exact_number_search()
        {
            var db = GetDbForSearchTestsWithDigits();
            using (db)
            {
                var helper = new RecordQueryer(db);
                var input = new RecordQueryInputModel
                {
                    Q = @"""1234""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };
                var results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>1234</b>");
            }
        }

        [Test]
        public void test_normal_search_amongst_titles_with_digits()
        {
            var db = GetDbForSearchTestsWithDigits();
            using (db)
            {
                var helper = new RecordQueryer(db);
                var input = new RecordQueryInputModel
                {
                    Q = "ABCD",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };
                var results = helper.Search(input).Results;
                results.Count.Should().Be(5);
            }
        }

        [Test]
        public void test_exact_search_amongst_titles_with_digits()
        {
            var db = GetDbForSearchTestsWithDigits();
            using (db)
            {
                var helper = new RecordQueryer(db);
                var input = new RecordQueryInputModel
                {
                    Q = @"""ABCD""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };
                var results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>ABCD</b>");
            }
        }

        private IDocumentSession GetDbForSortTests()
        {
            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                var record1 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "sea";
                    m.Gemini.DatasetReferenceDate = "2017-10-10";
                });
                var record2 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "seabirds";
                    m.Gemini.DatasetReferenceDate = "2017-10-19";
                });
                var record3 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "birds";
                    m.Gemini.DatasetReferenceDate = "2017-10-15";
                });
                var record4 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "coastal birds";
                    m.Gemini.DatasetReferenceDate = "2017-10-17";
                });

                db.Store(record1);
                db.Store(record2);
                db.Store(record3);
                db.Store(record4);
                db.SaveChanges();

                Thread.Sleep(100);

                return db;
            }
        }

        private IDocumentSession GetDbForSearchTestsWithDigits()
        {
            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                var record1 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "ABCD1234";
                });
                var record2 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "ABCD0123";
                });
                var record3 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "ABCD5678";
                });
                var record4 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "ABCD";
                });
                var record5 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "ABCD 1234";
                });
                var record6 = SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "1234";
                });

                db.Store(record1);
                db.Store(record2);
                db.Store(record3);
                db.Store(record4);
                db.Store(record5);
                db.Store(record6);
                db.SaveChanges();

                Thread.Sleep(300);

                return db;
            }
        }
    }
}
