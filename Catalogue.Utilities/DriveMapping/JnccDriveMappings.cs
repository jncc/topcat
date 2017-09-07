using System.Collections.Generic;

namespace Catalogue.Utilities.DriveMapping
{
    public static class JnccDriveMappings
    {
        private static Dictionary<string, string> DriveMappings = new Dictionary<string, string>
        {
            { @"G:\", @"\\JNCC-CORPFILE\Corporate Apps\" },
            { @"J:\", @"\\JNCC-CORPFILE\gis\" },
            { @"P:\", @"\\JNCC-CORPFILE\Purchase-Logs\" },
            { @"Y:\", @"\\JNCC-CORPFILE\Teams and Staff\" },
            { @"Z:\", @"\\JNCC-CORPFILE\JNCC Corporate Data\" },
            { @"X:\", @"\\JNCC-CORPFILE\Marine Survey\" }
        };

        public static string GetUncPath(string filePath)
        {
            var mappedPath = filePath;

            foreach (KeyValuePair < string, string> mapping in DriveMappings)
            {
                mappedPath = mappedPath.Replace(mapping.Key, mapping.Value);
            }

            return mappedPath;
        }
    }
}
