using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Utilities.Spatial
{
    public class BoundingBoxUtility
    {
        public static string GetWkt(decimal north, decimal south, decimal east, decimal west)
        {
            return String.Format("POLYGON(({0:G7} {1:G7},{2:G7} {3:G7},{4:G7} {5:G7},{6:G7} {7:G7},{8:G7} {9:G7}))",
                west, south,
                east, south,
                east, north,
                west, north,
                west, south);
        }
    }


    class bounding_box_utility_tests
    {
        [Test]
        public void should_get_correct_wkt()
        {
            string wkt = BoundingBoxUtility.GetWkt(north: 40, south: 10, east: 60, west: 20);
            wkt.Should().Be("POLYGON((20 10,60 10,60 40,20 40,20 10))");
        }
    }
}
