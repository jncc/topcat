using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Utilities.Text
{
    public static class StringExtensions
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

        public static string TruncateNicely(this string input, int length)
        {
            // http://stackoverflow.com/questions/1613896/truncate-string-on-whole-words-in-net-c-sharp

            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }
    }
}
