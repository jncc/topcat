using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Catalogue.Data.Query;
using Catalogue.Web.Code;
using Catalogue.Web.Controllers;
using Catalogue.Web.Controllers.Export;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Export
{
    class export_controller_tests : DatabaseTestFixture
    {
        [Test]
        public void export_sanity_check()
        {
            var controller = new ExportController(Db, new RecordQueryer(Db));
            var input = new RecordQueryInputModel
            {
                Q = "",
                F = new FilterOptions{Keywords = new[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" }},
                P = 0,
                N = -1
            };

            var result = controller.Get(input);

            var task = (PushStreamContent) result.Content;
            string content = task.ReadAsStringAsync().Result;

            var matches = Regex.Matches(content, @"""""Value"""":""""GB\d{6}"""""); // match a mesh identifier e.g. GB000272
            matches.Count.Should().Be(189);  // the number of mesh records
        }
    }
}
