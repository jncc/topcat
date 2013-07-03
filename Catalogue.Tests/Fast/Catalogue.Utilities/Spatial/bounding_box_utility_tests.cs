using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Utilities.Spatial;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Fast.Catalogue.Utilities.Spatial
{
    class bounding_box_utility_tests
    {
        [Test]
        public void should_get_correct_wkt()
        {
            string wkt = BoundingBoxUtility.GetWkt(north:40, south:10, east:60, west:20);
            wkt.Should().Be("POLYGON((20 10,60 10,60 40,20 40,20 10))");
        }
    }
}
