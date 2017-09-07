using System.Collections.Generic;

namespace Catalogue.Utilities.PathMapping
{
    public static class JnccDriveMappings
    {
        private static Dictionary<string, string> DriveMappings = new Dictionary<string, string>
        {
            { @"X:\OffshoreSurvey\", @"\\JNCC-CORPFILE\Marine Survey\OffshoreSurvey\" },
            { @"Z:\", @"\\JNCC-CORPFILE\JNCC Corporate Data\" }
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
