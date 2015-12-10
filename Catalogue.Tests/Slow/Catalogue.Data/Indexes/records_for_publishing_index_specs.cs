using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Indexes
{
    class records_for_publishing_index_specs : DatabaseTestFixture
    {
        [Test, Explicit]
        public void should_be_able_to_get_records_for_publishing()
        {
            var record = Db.Load<Record>(new Guid("679434f5-baab-47b9-98e4-81c8e3a1a6f9"));
            
            record.Publication = new PublicationInfo { OpenData = new OpenDataPublicationInfo() };
            Db.SaveChanges();
            RavenUtility.WaitForIndexing(Db);

            var recordsForPublishing = Db.Query<RecordsForPublishingIndex.Result, RecordsForPublishingIndex>()
                //.Where(x => x.MetadataDate > x.LastSuccess)
                .ToList();

            recordsForPublishing.Count.Should().Be(1);
        }
    }
}
