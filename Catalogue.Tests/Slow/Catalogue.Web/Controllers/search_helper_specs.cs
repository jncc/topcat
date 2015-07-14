using System;
using System.Linq;
using System.Runtime.InteropServices;
using Catalogue.Data.Model;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using Catalogue.Web.Controllers;
using FluentAssertions;
using NUnit.Framework;
using Raven.Database.Indexing.Collation;

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
            var helper = new RecordQuerier(Db);

            var input = new RecordQueryInputModel
                {
                    Q = "",
                    K = new [] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" },
                    P = 0,
                    N = 25,
                    D = null
                };

            helper.SearchQuery(input).Results.Count().Should().Be(25);
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
                };

                Db.Store(record);
                Db.SaveChanges();
            }

            using (var db2 = ReusableDocumentStore.OpenSession())
            {
                var helper = new RecordQuerier(db2);

                var input = new RecordQueryInputModel
                {
                    Q = "",
                    K = null,
                    P = 0,
                    N = 25,
                    D = DateTime.Parse("2020-01-01")
                };

                helper.RecordQuery(input).Count().Should().Be(1);

            }
        }

    }
}
