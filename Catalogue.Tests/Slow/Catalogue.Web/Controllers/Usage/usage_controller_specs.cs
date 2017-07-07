using Catalogue.Web.Controllers.Usage;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Usage
{
    public class usage_controller_specs : DatabaseTestFixture
    {
        [Test]
        public void recently_modified_records()
        {
            var usageController = new UsageController(Db);
            var usageResult = usageController.GetRecentlyModifiedRecords();
            var recentlyModifiedRecords = usageResult.RecentlyModifiedRecords;

            recentlyModifiedRecords.Count.Should().Be(5);
            recentlyModifiedRecords[0].Title.Should().Be("DateTest_7");
            recentlyModifiedRecords[1].Title.Should().Be("DateTest_5");
            recentlyModifiedRecords[2].Title.Should().Be("DateTest_4");
            recentlyModifiedRecords[3].Title.Should().Be("DateTest_3");
            recentlyModifiedRecords[4].Title.Should().Be("DateTest_2");

            recentlyModifiedRecords[0].Date.Should().Be(new DateTime(2017, 07, 12, 15, 00, 00));
            recentlyModifiedRecords[1].Date.Should().Be(new DateTime(2017, 07, 11, 10, 00, 00));
            recentlyModifiedRecords[2].Date.Should().Be(new DateTime(2017, 07, 01, 14, 00, 00));
            recentlyModifiedRecords[3].Date.Should().Be(new DateTime(2017, 07, 01, 15, 00, 00));
            recentlyModifiedRecords[4].Date.Should().Be(new DateTime(2017, 07, 05, 09, 00, 00));

            recentlyModifiedRecords[0].User.Should().Be("Cathy");
            recentlyModifiedRecords[1].User.Should().Be("Pete");
            recentlyModifiedRecords[2].User.Should().Be("Pete");
            recentlyModifiedRecords[3].User.Should().Be("Cathy");
            recentlyModifiedRecords[4].User.Should().Be("Pete");
        }
    }
}