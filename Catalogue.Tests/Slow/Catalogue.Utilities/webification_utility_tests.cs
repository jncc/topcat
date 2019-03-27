using Catalogue.Utilities.Text;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Utilities
{
    public class webification_utility_tests
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
