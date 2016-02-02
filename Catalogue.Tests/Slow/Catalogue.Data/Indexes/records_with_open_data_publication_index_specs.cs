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
    class records_with_open_data_publication_index_specs : DatabaseTestFixture
    {
        List<Record> records;
        
        [SetUp]
        public void set_up()
        {
            // note: the MetadataDate for seeded records is 2015-01-01 12:00:00

            records = Db.Query<RecordsWithOpenDataPublicationIndex.Result, RecordsWithOpenDataPublicationIndex>()
//                .Where(x => x.LastPublicationAttemptDate == DateTime.MinValue )
                .Where(x => x.LastSuccessfulPublicationAttemptDate > x.LastPublicationAttemptDate)
//                    || x.LastSuccessfulPublicationAttemptDate > x.LastPublicationAttemptDate
//                    || x.MetadataDate > x.LastSuccessfulPublicationAttemptDate)
                .OfType<Record>()
                .ToList();
        }

        [Test, Explicit]
        public void should_be_able_to_get_records_for_publishing()
        {
            records.Count.Should().Be(1);
        }
    }
}
