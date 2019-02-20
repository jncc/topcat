using Catalogue.Data;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    public class records_with_publication_info_index_specs : SeededDbTest
    {
        // these tests rely on the seeded records - makes them a bit hard to understand - see AddRecordsWithPublishingInfo in Seeder.cs
        // note: the MetadataDate for seeded records is 2015-01-01 12:00:00

        [Test]
        public void should_be_able_to_get_all_records_with_publishing_info()
        {
            Db.Query<Record, RecordsWithPublicationInfoIndex>().Count().Should().Be(13);
        }
        
        [Test]
        public void should_be_able_to_get_never_attempted_records()
        {
            Db.Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Where(x => x.PublicationNeverAttempted)
                .Count().Should().Be(3);
        }

        [Test]
        public void should_be_able_to_get_the_unsuccessful_attempt_record()
        {
            var results = Db.Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Where(x => x.LastPublicationAttemptWasUnsuccessful)
                .OfType<Record>()
                .ToList();

            results.Count.Should().Be(1);
            results.Single().Id.Should().Be(Helpers.AddCollection("b2691fed-e421-4e48-9da9-99bd77e0b8ba"));
        }

        [Test]
        public void should_be_able_to_get_successful_attempt_records()
        {
            // in other words, the last publication was successful and the record hasn't been updated since!

            var results = Db.Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Where(x => x.PublishedToGovSinceLastUpdated)
                .OfType<Record>()
                .ToList();

            results.Count.Should().Be(2);
            results.Should().Contain(r => r.Id.ToString() == Helpers.AddCollection("471da4f2-d9e2-4a5a-b72b-3ae8cc40ae57"));
            results.Should().Contain(r => r.Id.ToString() == Helpers.AddCollection("d9c14587-90d8-4eba-b670-4cf36e45196d"));
        }

        [Test]
        public void should_be_able_to_get_updated_since_last_published_record()
        {
            // in other words, all the records that should be published on the next run
            var results = Db.Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Where(x => !x.PublishedToGovSinceLastUpdated)
                .OfType<Record>()
                .ToList();

            results.Count.Should().Be(11);
            results.Should().Contain(r => r.Id.ToString() == Helpers.AddCollection("19b8c7ab-5c33-4d55-bc1d-3762b8207a9f"));
        }

        [Test]
        public void should_be_able_to_get_published_since_last_updated()
        {
            var results = Db.Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Where(x => x.PublishedToGovSinceLastUpdated)
                .OfType<Record>()
                .ToList();

            results.Count.Should().Be(2);
            results.Should().Contain(r => r.Id.ToString() == Helpers.AddCollection("471da4f2-d9e2-4a5a-b72b-3ae8cc40ae57"));
            results.Should().Contain(r => r.Id.ToString() == Helpers.AddCollection("d9c14587-90d8-4eba-b670-4cf36e45196d"));
        }
    }
}
