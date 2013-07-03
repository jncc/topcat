using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
