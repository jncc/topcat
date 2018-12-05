using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Catalogue.Data.Query;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using Catalogue.Web.Code;
using Catalogue.Web.Controllers;
using Catalogue.Web.Controllers.Export;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents.Session;
using Raven.TestDriver;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Export
{
    public class export_controller_tests : SeededDbTest
    {
        [Test]
        public void export_sanity_check()
        {
            var controller = new ExportController(new RecordQueryer(Db));
            var input = new RecordQueryInputModel
            {
                Q = "",
                F = new FilterOptions { Keywords = new[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" } },
                P = 0,
                N = -1
            };

            var result = controller.Get(input, "tsv");

            var task = (PushStreamContent)result.Content;
            string content = task.ReadAsStringAsync().Result;

            var matches = Regex.Matches(content, @"""""Value"""":""""GB\d{6}"""""); // match a mesh identifier e.g. GB000272
            matches.Count.Should().Be(189);  // the number of mesh records
        }

        [Test]
        public void export_as_csv_sanity_check()
        {
            var controller = new ExportController(new RecordQueryer(Db));
            var input = new RecordQueryInputModel
            {
                Q = "",
                F = new FilterOptions { Keywords = new[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" } },
                P = 0,
                N = -1
            };

            var result = controller.Get(input, "CSV");

            var task = result.Content;

            var content = task.ReadAsStringAsync().Result;
            content.Should().NotBeNullOrWhiteSpace();

            var matches =
                Regex.Matches(content, @"""""Value"""":""""GB\d{6}"""""); // match a mesh identifier e.g. GB000272
            matches.Count.Should().Be(189); // the number of mesh records
        }

        [Test]
        public void export_with_format_filter()
        {
            var controller = new ExportController(new RecordQueryer(Db));
            var input = new RecordQueryInputModel
            {
                Q = "sea",
                F = new FilterOptions { DataFormats = new[] { "Other" } },
                P = 0,
                N = 15
            };

            var result = controller.Get(input, "csv");

            var task = (PushStreamContent)result.Content;
            string content = task.ReadAsStringAsync().Result;
            content.Should().Contain("A simple Overseas Territories example record");
            Regex.Matches(content, @"\r\n").Count.Should().Be(2); // 2 new lines means only one record
        }
    }
}
