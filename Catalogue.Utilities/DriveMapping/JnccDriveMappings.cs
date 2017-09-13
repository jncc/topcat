using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Catalogue.Utilities.DriveMapping
{
    public static class JnccDriveMappings
    {
        private static readonly Dictionary<string, string> DriveMappings = new Dictionary<string, string>
        {
            { @"G:", @"\\JNCC-CORPFILE\Corporate Apps" },
            { @"J:", @"\\JNCC-CORPFILE\gis" },
            { @"P:", @"\\JNCC-CORPFILE\Purchase-Logs" },
            { @"Y:", @"\\JNCC-CORPFILE\Teams and Staff" },
            { @"Z:", @"\\JNCC-CORPFILE\JNCC Corporate Data" },
            { @"H:", @"\\JNCC-ABNAS2\private" },
            { @"L:", @"\\JNCC-ABNAS\marine-data" },
            { @"R:", @"\\JNCC-ABNAS2\reference_material" },
            { @"T:", @"\\JNCC-ABNAS2\cfs" },
            { @"X:", @"\\JNCC-CORPFILE\Marine Survey" }
        };

        public static string GetUncPath(string filePath)
        {
            var mappedPath = filePath;

            foreach (KeyValuePair < string, string> mapping in DriveMappings)
            {
                mappedPath = Regex.Replace(mappedPath, mapping.Key, mapping.Value, RegexOptions.IgnoreCase);
            }

            return mappedPath;
        }
    }
}
