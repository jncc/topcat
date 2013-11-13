using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Utilities.Spatial
{
    public class BoundingBoxUtility
    {
        public static string ToWkt(BoundingBox box)
        {
            return String.Format("POLYGON(({0:G7} {1:G7},{2:G7} {3:G7},{4:G7} {5:G7},{6:G7} {7:G7},{8:G7} {9:G7}))",
                box.West, box.South,
                box.East, box.South,
                box.East, box.North,
                box.West, box.North,
                box.West, box.South);
        }
    }


    class bounding_box_utility_tests
    {
        [Test]
        public void should_create_correct_wkt()
        {
            var box = new BoundingBox { North = 40, South = 10, East = 60, West = 20 };
            string wkt = BoundingBoxUtility.ToWkt(box);
            wkt.Should().Be("POLYGON((20 10,60 10,60 40,20 40,20 10))");
        }

        [Test]
        public void blah()
        {
            var box = new BoundingBox {North = 30, South = 20, East = 60, West = 50};
            BoundingBoxUtility.ToWkt(box).Should().Be("POLYGON((50 20,60 20,60 30,50 30,50 20))");
        }
    }
}
