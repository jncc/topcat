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
            var timeToTest = new DateTime(2017, 07, 12, 17, 00, 00);
            var usageResult = usageController.GetRecentlyModifiedRecords(timeToTest);
            var recentlyModifiedRecords = usageResult.RecentlyModifiedRecords;

            recentlyModifiedRecords.Count.Should().Be(5);
            recentlyModifiedRecords[0].Gemini.Title.Should().Be("DateTest_7");
            recentlyModifiedRecords[1].Gemini.Title.Should().Be("DateTest_5");
            recentlyModifiedRecords[2].Gemini.Title.Should().Be("DateTest_4");
            recentlyModifiedRecords[3].Gemini.Title.Should().Be("DateTest_3");
            recentlyModifiedRecords[4].Gemini.Title.Should().Be("DateTest_2");

            recentlyModifiedRecords[0].Gemini.MetadataDate.Should().Be(new DateTime(2017, 07, 12, 15, 00, 00));
            recentlyModifiedRecords[1].Gemini.MetadataDate.Should().Be(new DateTime(2017, 07, 11, 10, 00, 00));
            recentlyModifiedRecords[2].Gemini.MetadataDate.Should().Be(new DateTime(2017, 07, 01, 14, 00, 00));
            recentlyModifiedRecords[3].Gemini.MetadataDate.Should().Be(new DateTime(2017, 07, 01, 15, 00, 00));
            recentlyModifiedRecords[4].Gemini.MetadataDate.Should().Be(new DateTime(2017, 07, 05, 09, 00, 00));

            recentlyModifiedRecords[0].Gemini.MetadataPointOfContact.Should().Be("Cathy");
            recentlyModifiedRecords[1].Gemini.MetadataPointOfContact.Should().Be("Pete");
            recentlyModifiedRecords[2].Gemini.MetadataPointOfContact.Should().Be("Pete");
            recentlyModifiedRecords[3].Gemini.MetadataPointOfContact.Should().Be("Cathy");
            recentlyModifiedRecords[4].Gemini.MetadataPointOfContact.Should().Be("Pete");
        }
    }
}