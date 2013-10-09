using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Tests.Utility;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;

namespace Catalogue.Tests.Explicit
{
    class exploratory_search_tests : DatabaseTestFixture
    {
        [Explicit, Test]
        public void can_search()
        {
            // sanity check the seed data has loaded
            Db.Query<Record>().Count().Should().BeGreaterThan(100);

            string q = "broad bio";
            var list = Db.Advanced.LuceneQuery<Record>("Records/Search")
                .Search("Title", q + "*").Boost(10)
                .ToList();

            foreach (var record in list)
            {
                Console.WriteLine(record.Gemini.Title);
            }

            list.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_able_to_highlight_search_terms()
        {
            FieldHighlightings lites;
            string q = "north";

            var results = Db.Advanced.LuceneQuery<Record>("Records/Search")
                                .Highlight("Title", 128, 2, out lites)
                                .SetHighlighterTags("<strong>", "</strong>")
                                .Search("Title", q).Boost(10)
                                .Take(10)
                                .ToList();

            results.Count.Should().Be(10);
            lites.FieldName.Should().Be("Title");
            lites.GetFragments("records/" + results.First().Id).First().Should().ContainEquivalentOf("<strong>north</strong>");

        }
    }
}
