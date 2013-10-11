using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Utilities.Text
{
    public static class StringExtensions
    {
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
