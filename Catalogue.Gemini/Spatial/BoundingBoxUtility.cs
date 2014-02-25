using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Gemini.Spatial
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

        public static bool IsBlank(BoundingBox box)
        {
            return box.North == 0 && box.South == 0 && box.East == 0 && box.West == 0;
        }
    }


    class bounding_box_utility_tests
    {
        [Test]
        public void can_create_correct_wkt()
        {
            var box = new BoundingBox { North = 40, South = 10, East = 60, West = 20 };
            string wkt = BoundingBoxUtility.ToWkt(box);
            wkt.Should().Be("POLYGON((20 10,60 10,60 40,20 40,20 10))");
        }

        [Test]
        public void can_identify_a_blank_box()
        {
            var box = new BoundingBox { North = 0, South = 0, East = 0, West = 0 };
            BoundingBoxUtility.IsBlank(box).Should().BeTrue();
        }
    }

}
