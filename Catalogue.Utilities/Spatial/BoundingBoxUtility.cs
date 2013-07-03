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
            return String.Format("POLYGON(({0} {1},{2} {3},{4} {5},{6} {7},{8} {9}))",
                west, south,
                east, south,
                east, north,
                west, north,
                west, south);
        }
    }
}
