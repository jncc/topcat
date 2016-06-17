using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Utilities.Time;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    class records_with_open_data_publication_info_index_specs : DatabaseTestFixture
    {
        // these tests rely on the seeded records - makes them a bit hard to understand - see AddRecordsWithPublishingInfo in Seeder.cs
        // note: the MetadataDate for seeded records is 2015-01-01 12:00:00

        [Test]
        public void should_be_able_to_get_all_records_with_publishing_info()
        {
            Db.Query<Record, RecordsWithOpenDataPublicationInfoIndex>().Count().Should().Be(5);
        }
        
        [Test]
        public void should_be_able_to_get_never_attempted_records()
        {
            Db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.PublicationNeverAttempted)
                .Count().Should().Be(2);
        }

        [Test]
        public void should_be_able_to_get_the_unsuccessful_attempt_record()
        {
            var results = Db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.LastPublicationAttemptWasUnsuccessful)
                .OfType<Record>()
                .ToList();

            results.Count().Should().Be(1);
            results.Single().Id.Should().Be("b2691fed-e421-4e48-9da9-99bd77e0b8ba");
        }

        [Test]
        public void should_be_able_to_get_the_successful_attempt_record()
        {
            // in other words, the last publication was successful and the record hasn't been updated since!

            var results = Db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.PublishedSinceLastUpdated)
                .OfType<Record>()
                .ToList();

            results.Count().Should().Be(1);
            results.Single().Id.Should().Be("d9c14587-90d8-4eba-b670-4cf36e45196d");
        }

        [Test]
        public void should_be_able_to_get_updated_since_last_published_record()
        {
            // in other words, all the records that should be published on the next run
            var results = Db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => !x.PublishedSinceLastUpdated)
                .OfType<Record>()
                .ToList();

            results.Count().Should().Be(4);
            results.Should().Contain( r => r.Id.ToString() == "19b8c7ab-5c33-4d55-bc1d-3762b8207a9f");
        }

        [Test]
        public void should_be_able_to_get_non_paused_records()
        {
            var results = Db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => !x.PublishingIsPaused)
                .OfType<Record>()
                .ToList();

            results.Count().Should().Be(4);
        }
    }
}
