using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var controller = new ExportController(new RecordQueryer(Db));
            var input = new RecordQueryInputModel
            {
                Q = "",
                K = new[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps" },
                P = 0,
                N = 25,
            };

            var result = controller.FetchRecords(input);
            result.Should().HaveCount(189); // the number of mesh records
        }
    }
}
