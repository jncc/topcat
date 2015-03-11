using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Catalogue.Utilities.Text
{
    public static class StringUtility
    {
        public static bool IsBlank(this string s)
        {
            return String.IsNullOrWhiteSpace(s);
        }

        public static bool IsNotBlank(this string s)
        {
            return !String.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// Creates a string from the sequence by concatenating the elements.
        /// </summary>
        public static string ToConcatenatedString(this IEnumerable<string> source, string separator)
        {
            return source.ToConcatenatedString(s => s, separator);
        }

        /// <summary>
        /// Creates a string from the sequence by concatenating the result
        /// of the specified string selector function for each element.
        /// </summary>
        public static string ToConcatenatedString<T>(this IEnumerable<T> source,
            Func<T, string> stringSelector)
        {
            return source.ToConcatenatedString(stringSelector, String.Empty);
        }

        /// <summary>
        /// Creates a string from the sequence by concatenating the result
        /// of the specified string selector function for each element.
        /// </summary>
        ///<param name="separator">The string which separates each concatenated item.</param>
        public static string ToConcatenatedString<T>(this IEnumerable<T> source,
            Func<T, string> stringSelector,
            string separator)
        {
            var b = new StringBuilder();
            bool needsSeparator = false; // don't use for first item

            foreach (var item in source)
            {
                if (needsSeparator)
                    b.Append(separator);

                b.Append(stringSelector(item));
                needsSeparator = true;
            }

            return b.ToString();
        }

        public static string Truncate(this string input, int length)
        {
            if (input.IsBlank())
                return String.Empty;
            else if (input.Length <= length)
                return input;
            else
                return input.Substring(0, length);
        }

        public static string TruncateNicely(this string input, int length)
        {
            // http://stackoverflow.com/questions/1613896/truncate-string-on-whole-words-in-net-c-sharp

            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }

        public static string FirstCharToLower(this string s)
        {
            if (String.IsNullOrEmpty(s))
                return s;
            if (s.Length == 1)
                return s.ToLowerInvariant();
            return s.Remove(1).ToLowerInvariant() + s.Substring(1);
        }

        public static string ToCamelCase(string s)
        {
            // https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Utilities/StringUtils.cs

            if (string.IsNullOrEmpty(s))
                return s;

            if (!char.IsUpper(s[0]))
                return s;

            var sb = new StringBuilder();
            
            for (int i = 0; i < s.Length; i++)
            {
                bool hasNext = (i + 1 < s.Length);

                if ((i == 0 || !hasNext) || char.IsUpper(s[i + 1]))
                {
                    char lowerCase = char.ToLower(s[i], CultureInfo.InvariantCulture);
                    sb.Append(lowerCase);
                }
                else
                {
                    sb.Append(s.Substring(i));
                    break;
                }
            }

            return sb.ToString();
        }

        public static string FirstCharToUpper(this string s)
        {
            if (s.IsNotBlank())
                return s.First().ToString().ToUpper() + s.Substring(1);
            else
                return s;
        }
    }

    class string_utility_tests
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
