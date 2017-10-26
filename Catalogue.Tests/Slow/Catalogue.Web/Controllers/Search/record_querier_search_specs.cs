using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Test;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using System;
using System.Linq;
using static Catalogue.Data.Query.RecordQueryInputModel.SortOptions;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Search
{
    class record_querier_search_specs : DatabaseTestFixture
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
        public void test_simple_search()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "sea",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(4);
                results.ToList()[0].Title.Should().Be("<b>sea</b>");
                results.ToList()[1].Title.Should().Be("<b>sea</b>1234");
                results.ToList()[2].Title.Should().Be("<b>sea</b> 1234");
                results.ToList()[3].Title.Should().Be("<b>sea</b>birds");
            }
        }

        [Test]
        public void test_simple_search_with_multiple_terms_and_numbers()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "sea 34",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("<b>sea</b>1234");
                results.ToList()[1].Title.Should().Be("<b>sea</b> 1234");
                results.ToList()[2].Title.Should().Be("<b>sea</b>");
            }
        }

        [Test]
        public void test_simple_search_with_multiple_terms()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "coastal bird",
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
        public void test_exact_search_where_title_has_multiple_terms()
        {
            using (var db = GetDbForSortTests())
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
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>coastal</b> birds");
            }
        }

        [Test]
        public void test_exact_search_when_title_has_multiple_terms_and_digits()
        {
            using (var db = GetDbForSortTests())
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
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("<b>sea</b>");
                results.ToList()[1].Title.Should().Be("<b>sea</b>1234");
                results.ToList()[2].Title.Should().Be("<b>sea</b> 1234");
            }
        }

        [Test]
        public void test_exact_search_query_works_with_sorting()
        {
            using (var db = GetDbForSortTests())
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
                results.Count.Should().Be(2);
                results.ToList()[0].Title.Should().Be("<b>birds</b>");
                results.ToList()[1].Title.Should().Be("coastal <b>birds</b>");
            }
        }

        [Test]
        public void test_exact_search_with_no_matches()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""coast""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(0);
            }
        }

        [Test]
        public void test_exact_search_of_phrase()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""coastal birds""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("<b>coastal birds</b>");
            }
        }

        [Test]
        public void test_search_is_case_insensitive()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "BIRDS",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = TitleAZ
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("<b>birds</b>");
                results.ToList()[1].Title.Should().Be("coastal <b>birds</b>");
                results.ToList()[2].Title.Should().Be("sea<b>birds</b>");
            }
        }

        [Test]
        public void test_exact_search_is_case_insensitive()
        {
            using (var db = GetDbForSortTests())
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
                results.Count.Should().Be(2);
                results.ToList()[0].Title.Should().Be("<b>birds</b>");
                results.ToList()[1].Title.Should().Be("coastal <b>birds</b>");
            }
        }

        [Test]
        public void test_multiple_exact_search_terms()
        {
            using (var db = GetDbForSortTests())
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
        public void test_order_matters_in_exact_search()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""birds coastal""",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(0);
            }
        }

        [Test]
        public void test_exact_search_term_with_normal_search_term()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = @"""coastal"" bird",
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
        public void test_number_only_search()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);
                var input = new RecordQueryInputModel
                {
                    Q = "123",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };
                var results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("<b>123</b>4");
                results.ToList()[1].Title.Should().Be("sea<b>123</b>4");
                results.ToList()[2].Title.Should().Be("sea <b>123</b>4");
            }
        }

        [Test]
        public void test_simple_search_with_numbers_and_letters()
        {
            using (var db = GetDbForSortTests())
            {
                var helper = new RecordQueryer(db);
                var input = new RecordQueryInputModel
                {
                    Q = "sea12",
                    K = null,
                    P = 0,
                    N = 25,
                    D = null,
                    O = MostRelevant
                };
                var results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("<b>sea</b>1234");
                results.ToList()[1].Title.Should().Be("<b>sea</b>");
                results.ToList()[2].Title.Should().Be("<b>sea</b> 1234");
            }
        }

        private IDocumentSession GetDbForSortTests()
        {
            var store = new InMemoryDatabaseHelper().Create();
            var db = store.OpenSession();
            
            var record1 = SimpleRecord().With(m =>
            {
                m.Gemini.Title = "sea";
            });
            var record2 = SimpleRecord().With(m =>
            {
                m.Gemini.Title = "seabirds";
            });
            var record3 = SimpleRecord().With(m =>
            {
                m.Gemini.Title = "birds";
            });
            var record4 = SimpleRecord().With(m =>
            {
                m.Gemini.Title = "coastal birds";
            });
            var record5 = SimpleRecord().With(m =>
            {
                m.Gemini.Title = "1234";
            });
            var record6 = SimpleRecord().With(m =>
            {
                m.Gemini.Title = "sea1234";
            });
            var record7 = SimpleRecord().With(m =>
            {
                m.Gemini.Title = "sea 1234";
            });

            db.Store(record1);
            db.Store(record2);
            db.Store(record3);
            db.Store(record4);
            db.Store(record5);
            db.Store(record6);
            db.Store(record7);
            db.SaveChanges();

            return db;
        }
    }
}
