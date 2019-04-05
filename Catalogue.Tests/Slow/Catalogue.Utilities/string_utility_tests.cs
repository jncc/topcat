using Catalogue.Utilities.Text;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Utilities
{
    public class string_utility_tests
    {
        [Test]
        public void test_to_camel_case()
        {
            // https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json.Tests/Utilities/StringUtilsTests.cs
            Assert.AreEqual("urlValue", StringUtility.ToCamelCase("URLValue"));
            Assert.AreEqual("url", StringUtility.ToCamelCase("URL"));
            Assert.AreEqual("id", StringUtility.ToCamelCase("ID"));
            Assert.AreEqual("i", StringUtility.ToCamelCase("I"));
            Assert.AreEqual("", StringUtility.ToCamelCase(""));
            Assert.AreEqual(null, StringUtility.ToCamelCase(null));
            Assert.AreEqual("iPhone", StringUtility.ToCamelCase("iPhone"));
            Assert.AreEqual("person", StringUtility.ToCamelCase("Person"));
            Assert.AreEqual("iPhone", StringUtility.ToCamelCase("IPhone"));
            Assert.AreEqual("i Phone", StringUtility.ToCamelCase("I Phone"));
            Assert.AreEqual(" IPhone", StringUtility.ToCamelCase(" IPhone"));
        }
    }
}
