using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (box == null)
                return true;

            return box.North == 0 && box.South == 0 && box.East == 0 && box.West == 0;
        }

        public static BoundingBox MinimumOf(IEnumerable<BoundingBox> boxes)
        {
            decimal northmost = boxes.Max(b => b.North);
            decimal southmost = boxes.Min(b => b.South);
            decimal eastmost  = boxes.Max(b => b.East);
            decimal westmost  = boxes.Min(b => b.West);

            return new BoundingBox { North = northmost, South = southmost, East = eastmost, West = westmost };
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

    class bounding_box_utility_minimum_of_tests
    {

        [Test]
        public void should_be_correct_for_single_box()
        {
            var box = new BoundingBox { North = 21.9259m, West = -63.8981m, South = 17.9476m, East = -60.6832m };

            var min = BoundingBoxUtility.MinimumOf(new[] { box });

            min.North.Should().Be(box.North);
            min.West.Should().Be(box.West);
        }

        [Test]
        public void should_be_correct_for_one_box_containing_the_other()
        {
            var outer = new BoundingBox { North = 60.49854m, West = -10.05288m, South = 49.7122m, East = 1.46442m };
            var inner = new BoundingBox { North = 53.57077m, West = 5.295278m, South = 53.35306m, East = 5.17534m };

            var min = BoundingBoxUtility.MinimumOf(new[] { inner, outer });

            min.North.Should().Be(outer.North);
            min.South.Should().Be(outer.South);
            min.East.Should().Be(outer.East);
            min.West.Should().Be(outer.West);
        }

        [Test]
        public void should_be_correct_for_one_box_overlapping_the_other()
        {
            var southernIrishSea = new BoundingBox { North = 53.65873m, West = -6.967625m, South = 51.23337m, East = -3.950034m };
            var englishChannel = new BoundingBox { North = 52.065138m, West = -9.793268m, South = 48.264071m, East = 1.939627m };

            var min = BoundingBoxUtility.MinimumOf(new[] { southernIrishSea, englishChannel });

            // the southern irish sea is to the north of the english channel
            // but the english channel spans further to the east and west

            min.North.Should().Be(southernIrishSea.North);
            min.South.Should().Be(englishChannel.South);
            min.East.Should().Be(englishChannel.East);
            min.West.Should().Be(englishChannel.West);
        }
    }

}
