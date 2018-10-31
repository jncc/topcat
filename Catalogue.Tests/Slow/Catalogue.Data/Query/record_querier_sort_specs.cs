using System.Linq;
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
using Raven.Client.Documents.Session;

namespace Catalogue.Tests.Slow.Catalogue.Data.Query
{
    class record_querier_sort_specs : DatabaseTestFixture
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
        public void test_results_sorted_by_relevance()
        {
            var db = GetDbForSortTests();
            using (db)
            {
                var helper = new RecordQueryer(db);

                var input = new RecordQueryInputModel
                {
                    Q = "birds",
                    F = null,
                    P = 0,
                    N = 25,
                    O = SortOptions.MostRelevant
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
                    F = null,
                    P = 0,
                    N = 25,
                    O = SortOptions.TitleAZ
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
                    F = null,
                    P = 0,
                    N = 25,
                    O = SortOptions.TitleZA
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
                    F = null,
                    P = 0,
                    N = 25,
                    O = SortOptions.NewestToOldest
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
                    F = null,
                    P = 0,
                    N = 25,
                    O = SortOptions.OldestToNewest
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
                WaitForIndexing(ReusableDocumentStore);

                var input = new RecordQueryInputModel
                {
                    Q = "sea",
                    F = null,
                    P = 0,
                    N = 25,
                    O = SortOptions.TitleAZ
                };

                var results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("cow<b>sea</b>");
                results.ToList()[1].Title.Should().Be("<b>sea</b>");
                results.ToList()[2].Title.Should().Be("<b>sea</b>birds");

                input = new RecordQueryInputModel
                {
                    Q = "sea",
                    F = null,
                    P = 0,
                    N = 25,
                    O = SortOptions.TitleZA
                };

                results = helper.Search(input).Results;
                results.Count.Should().Be(3);
                results.ToList()[0].Title.Should().Be("<b>sea</b>birds");
                results.ToList()[1].Title.Should().Be("<b>sea</b>");
                results.ToList()[2].Title.Should().Be("cow<b>sea</b>");
            }
        }

        private IDocumentSession GetDbForSortTests()
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

            Db.Store(record1);
            Db.Store(record2);
            Db.Store(record3);
            Db.Store(record4);
            Db.SaveChanges();
            WaitForIndexing(ReusableDocumentStore);

            return Db;
        }
    }
}
