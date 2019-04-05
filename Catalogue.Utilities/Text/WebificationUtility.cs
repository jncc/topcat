using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Catalogue.Utilities.Text
{
    public static class WebificationUtility
    {
        /// <summary>
        /// Makes a "web-friendly" string suitable for us in URLs.
        /// For example, "Three Little words! - v2.txt" -> "Three-Little-words-v2.txt"
        /// </summary>
        public static string ToUrlFriendlyString(string s)
        {
            if (s.IsBlank())
                return s;

            // match "words" of lower and uppercase letters, numbers and dots
            var words = Regex.Matches(s, @"[A-Za-z0-9\.]+")
                .OfType<Match>()
                .Select(m => m.Value);

            // combine words with a "-", unless the second starts with a dot (we want "file.txt", not "file-.txt")
            return words.Aggregate((a, b) => a + (b.StartsWith(".") ? b : "-" + b));
        }

        /// <summary>
        /// Gets a path like "/data/3148e1e2-bd6b-4623-b72a-5408263b9056/Some-Data-File.csv"
        /// </summary>
        public static string GetUnrootedDataPath(string recordId, string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            // make a file name suitable for the web
            string name = ToUrlFriendlyString(fileName);

            return String.Format("/data/{0}/{1}", recordId, name);
        }
    }
}
