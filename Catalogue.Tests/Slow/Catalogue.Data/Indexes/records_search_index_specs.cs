using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    public class records_search_index_specs : SeededDbTest
    {
        [Test]
        public void can_search_partial_matches()
        {
            var results = Db.Advanced.DocumentQuery<Record, RecordIndex>()
                .Search("TitleN", "stu") // search the ngrammed title field for 'stu'
                .Take(100)
                .ToList();

            results.Count.Should().Be(13);
            results.Any(r => r.Gemini.Title.Contains("Study")).Should().BeTrue();
            results.Any(r => r.Gemini.Title.Contains("Estuaries")).Should().BeTrue();
        }
    }
}