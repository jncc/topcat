using NUnit.Framework;
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
        /// Gets a path like "data/3148e1e2-bd6b-4623-b72a-5408263b9056-Some-Data-File.csv"
        /// </summary>
        public static string GetUnrootedDataPath(string recordId, string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            // make a file name suitable for the web
            string name = WebificationUtility.ToUrlFriendlyString(fileName);

            return String.Format("data/{0}-{1}", recordId, name);
        }
    }

    class webification_utility_tests
    {
        [TestCase("two little words", ExpectedResult = "two-little-words")]
        [TestCase("Combined_P_A_Matrix)data_sources.csv", ExpectedResult = "Combined-P-A-Matrix-data-sources.csv")]
        [TestCase("Three Little words! - v2.txt", ExpectedResult = "Three-Little-words-v2.txt")]
        [TestCase("New-York NEW-YORK!.txt", ExpectedResult = "New-York-NEW-YORK.txt")]
        [TestCase("my.file.txt", ExpectedResult = "my.file.txt")]
        [TestCase("myfile.txt", ExpectedResult = "myfile.txt")]
        [TestCase("Sound_Strait_WGS84.zip", ExpectedResult = "Sound-Strait-WGS84.zip")]
        [TestCase("", ExpectedResult = "")]
        [TestCase(null, ExpectedResult = null)]
        public string test_to_url_friendly_string(string s)
        {
            return WebificationUtility.ToUrlFriendlyString(s);
        }
    }
}
