using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    internal class records_search_index_specs : DatabaseTestFixture
    {
        [Test]
        public void can_search_partial_matches()
        {
            List<Record> results = Db.Advanced.LuceneQuery<Record, RecordIndex>()
                .Search("TitleN", "stu") // search the ngrammed title field for 'stu'
                .Take(100)
                .ToList();

            results.Count.Should().Be(13);
            results.Any(r => r.Gemini.Title.Contains("Study")).Should().BeTrue();
            results.Any(r => r.Gemini.Title.Contains("Estuaries")).Should().BeTrue();
        }
    }
}