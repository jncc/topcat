using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Test;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;

namespace Catalogue.Tests.Slow.Catalogue.Data.Query
{
    class record_querier_search_specs : CleanDbTest
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

            var record1 = QueryTestHelper.SimpleRecord().With(m => { m.Gemini.Title = "sea"; });
            var record2 = QueryTestHelper.SimpleRecord().With(m => { m.Gemini.Title = "seabirds"; });
            var record3 = QueryTestHelper.SimpleRecord().With(m => { m.Gemini.Title = "birds"; });
            var record4 = QueryTestHelper.SimpleRecord().With(m => { m.Gemini.Title = "coastal birds"; });
            var record5 = QueryTestHelper.SimpleRecord().With(m => { m.Gemini.Title = "1234"; });
            var record6 = QueryTestHelper.SimpleRecord().With(m => { m.Gemini.Title = "sea1234"; });
            var record7 = QueryTestHelper.SimpleRecord().With(m => { m.Gemini.Title = "sea 1234"; });

            Db.Store(record1);
            Db.Store(record2);
            Db.Store(record3);
            Db.Store(record4);
            Db.Store(record5);
            Db.Store(record6);
            Db.Store(record7);
            Db.SaveChanges();
            WaitForIndexing(ReusableDocumentStore);
        }

        [Test]
        public void test_simple_search()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = "sea");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(4);
            results.ToList()[0].Title.Should().Be("<b>sea</b>");
            results.ToList()[1].Title.Should().Be("<b>sea</b>1234");
            results.ToList()[2].Title.Should().Be("<b>sea</b> 1234");
            results.ToList()[3].Title.Should().Be("<b>sea</b>birds");
        }

        [Test, Ignore("Only works if we decide to AND search terms instead of ORing")]
        public void test_simple_search_with_multiple_terms_and_numbers()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = "sea 34");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(3);
            results.ToList()[0].Title.Should().Be("<b>sea</b>1234");
            results.ToList()[1].Title.Should().Be("<b>sea</b> 1234");
            results.ToList()[2].Title.Should().Be("<b>sea</b>");
        }

        [Test, Ignore("Only works if we decide to AND search terms instead of ORing")]
        public void test_simple_search_with_multiple_terms()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = "coastal bird");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(1);
            results.ToList()[0].Title.Should().Be("<b>coastal</b> <b>birds</b>");
        }

        [Test]
        public void test_exact_search_where_title_has_multiple_terms()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""coastal""");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(1);
            results.ToList()[0].Title.Should().Be("<b>coastal</b> birds");
        }

        [Test, Ignore("raven4")]
        public void test_exact_search_when_title_has_multiple_terms_and_digits()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""sea""");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(3);
            results.ToList()[0].Title.Should().Be("<b>sea</b>");
            results.ToList()[1].Title.Should().Be("<b>sea</b>1234");
            results.ToList()[2].Title.Should().Be("<b>sea</b> 1234");
        }

        [Test, Ignore("raven4")]
        public void test_exact_search_query_works_with_sorting()
        {
            var helper = new RecordQueryer(Db);

            var input = new RecordQueryInputModel
            {
                Q = @"""birds""",
                F = null,
                P = 0,
                N = 25,
                O = SortOptions.TitleAZ
            };

            var results = helper.Search(input).Results;
            results.Count.Should().Be(2);
            results.ToList()[0].Title.Should().Be("<b>birds</b>");
            results.ToList()[1].Title.Should().Be("coastal <b>birds</b>");
        }

        [Test, Ignore("raven4")]
        public void test_exact_search_with_no_matches()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""coast""");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(0);
        }

        //raven4
        [Test, Ignore("Need to work out how to do this in raven 4")]
        public void test_exact_search_of_phrase()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""coastal birds""");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(1);
            results.ToList()[0].Title.Should().Be("<b>coastal birds</b>");
        }

        [Test]
        public void test_search_is_case_insensitive()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => { x.Q = "BIRDS"; x.O = SortOptions.TitleAZ; });

            var results = helper.Search(input).Results;
            results.Count.Should().Be(3);
            results.ToList()[0].Title.Should().Be("<b>birds</b>");
            results.ToList()[1].Title.Should().Be("coastal <b>birds</b>");
            results.ToList()[2].Title.Should().Be("sea<b>birds</b>");
        }

        [Test, Ignore("raven4")]
        public void test_exact_search_is_case_insensitive()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => { x.Q = @"""BIRDS"""; x.O = SortOptions.TitleAZ; });

            var results = helper.Search(input).Results;
            results.Count.Should().Be(2);
            results.ToList()[0].Title.Should().Be("<b>birds</b>");
            results.ToList()[1].Title.Should().Be("coastal <b>birds</b>");
        }

        [Test, Ignore("Only works if we decide to AND search terms instead of ORing")]
        public void test_multiple_exact_search_terms()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""birds"" ""coastal""");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(1);
            results.ToList()[0].Title.Should().Be("<b>coastal</b> <b>birds</b>");
        }

        [Test, Ignore("raven4")]
        public void test_order_matters_in_exact_search()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""birds coastal""");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(0);
        }

        [Test, Ignore("Only works if we decide to AND search terms instead of ORing")]
        public void test_exact_search_term_with_normal_search_term()
        {
            var helper = new RecordQueryer(Db);

            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""coastal"" bird");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(1);
            results.ToList()[0].Title.Should().Be("<b>coastal</b> <b>birds</b>");
        }

        [Test]
        public void test_number_only_search()
        {
            var helper = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = "123");
            var results = helper.Search(input).Results;
            results.Count.Should().Be(3);
            results.ToList()[0].Title.Should().Be("<b>123</b>4");
            results.ToList()[1].Title.Should().Be("sea<b>123</b>4");
            results.ToList()[2].Title.Should().Be("sea <b>123</b>4");
        }

        [Test]
        public void test_simple_search_with_numbers_and_letters()
        {
            var helper = new RecordQueryer(Db);
            var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = "sea12");

            var results = helper.Search(input).Results;
            results.Count.Should().Be(3);
            results.ToList()[0].Title.Should().Be("<b>sea</b>1234");
            results.ToList()[1].Title.Should().Be("<b>sea</b>");
            results.ToList()[2].Title.Should().Be("<b>sea</b> 1234");
        }

        [Test, Ignore("Need to work out how to do this in raven4")]
        public void test_exact_search_checks_abstract_too()
        {
            using (var db = ReusableDocumentStore.OpenSession())
            {
                var record1 = QueryTestHelper.SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "marine conservation zone";
                    m.Gemini.Abstract = "population analysis data for zone";
                });
                var record2 = QueryTestHelper.SimpleRecord().With(m =>
                {
                    m.Gemini.Title = "habitat data";
                    m.Gemini.Abstract = "marine conservation analysis data";
                });

                db.Store(record1);
                db.Store(record2);
                db.SaveChanges();
                WaitForIndexing(ReusableDocumentStore);

                var helper = new RecordQueryer(db);

                var input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""marine conservation""");

                var results = helper.Search(input).Results;
                results.Count.Should().Be(2);
                results.ToList()[0].Title.Should().Be("<b>marine conservation</b> zone");
                results.ToList()[1].Title.Should().Be("habitat data");

                input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""analysis data""");
                
                results = helper.Search(input).Results;
                results.Count.Should().Be(2);
                results.ToList()[0].Title.Should().Be("marine conservation zone");
                results.ToList()[1].Title.Should().Be("habitat data");

                input = QueryTestHelper.EmptySearchInput().With(x => x.Q = @"""population analysis""");

                results = helper.Search(input).Results;
                results.Count.Should().Be(1);
                results.ToList()[0].Title.Should().Be("marine conservation zone");
            }
        }
    }
}
