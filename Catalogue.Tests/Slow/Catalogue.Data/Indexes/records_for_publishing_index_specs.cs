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
    class records_for_publishing_index_specs : DatabaseTestFixture
    {
        [SetUp]
        public void set_up()
        {
        }

        [Test, Explicit]
        public void should_be_able_to_get_records_for_publishing()
        {
            var recordsForPublishing = Db.Query<RecordsForPublishingIndex.Result, RecordsForPublishingIndex>()
                .Where(x => x.MetadataDate >= new DateTime(2015, 1, 1))
                //.Where(x => x.LastSuccess > new DateTime(2015, 1, 1))
                .OfType<Record>()
                .ToList();

            recordsForPublishing.Count.Should().Be(2);
        }
    }
}
