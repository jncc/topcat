using System;
using System.Linq;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Utilities.Raven;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    public class record_count_for_keyword_index_specs : SeededDbTest
    {
        [Test]
        public void should_be_able_to_get_collection_record_counts()
        {
            var counts = Db.Query<RecordCountForKeywordIndex.Result, RecordCountForKeywordIndex>()
                .Customize(x => x.WaitForNonStaleResults())
                .ToList();

            counts.Count.Should().BeGreaterOrEqualTo(2);
            
            counts
                .Where(r => r.KeywordVocab == "http://vocab.jncc.gov.uk/jncc-category")
                .Where(r => r.KeywordValue == "Seabed Habitat Maps")
                .Select(r => r.RecordCount)
                .Single()
                .Should().Be(189);
        }

        [Test]
        public void should_not_count_keywords_in_different_vocabs()
        {
            // there are two seeded records with the keyword 'butterfly' but they're in different vocabs 
            var results = Db.Query<RecordCountForKeywordIndex.Result, RecordCountForKeywordIndex>()
                .Customize(x => x.WaitForNonStaleResults())
                .Where(x => x.KeywordValue == "butterfly")
                .ToList();

            results.Count.Should().Be(2);
        }

        [Test]
        public void should_be_able_to_get_all_keywords()
        {
            var results = Db.Query<RecordCountForKeywordIndex.Result, RecordCountForKeywordIndex>()
                .Customize(x => x.WaitForNonStaleResults())
                .Fetch(5000);

            Console.WriteLine(results.Count);
            results.Count.Should().BeGreaterThan(2);
        }


    }
}
