using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Documents.Queries.Highlighting;

namespace Catalogue.Tests.Explicit
{
    internal class exploratory_search_tests : DatabaseTestFixture
    {
        [Explicit, Test]
        public void can_search()
        {
            // sanity check the seed data has loaded
            Db.Query<Record>().Count().Should().BeGreaterThan(100);

            string q = "broad bio";
            List<Record> list = Db.Advanced.DocumentQuery<Record>("Records/SearchQuery")
                .Search("Title", q + "*").Boost(10)
                .ToList();

            foreach (Record record in list)
            {
                Console.WriteLine(record.Gemini.Title);
            }

            list.Count.Should().BeGreaterThan(0);
        }

        [Explicit, Test]
        public void should_be_able_to_highlight_search_terms()
        {
            Highlightings lites;

            var highlightingOptions = new HighlightingOptions
            {
                PreTags = new[] { "<b>" },
                PostTags = new[] { "</b>" }
            };

            List<Record> results = Db.Advanced.DocumentQuery<Record>("Records/SearchQuery")
                .Highlight("Title", 128, 2, highlightingOptions, out lites)
                .Search("Title", "north").Boost(10)
                .Take(10)
                .ToList();

            results.Count.Should().Be(10);
            lites.FieldName.Should().Be("Title");
            lites.GetFragments("records/" + results.First().Id)
                .First()
                .Should()
                .ContainEquivalentOf("<strong>north</strong>");
        }
    }
}