﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

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
            // match "words" of lower and uppercase letters, numbers and dot
            var words = Regex.Matches(s, @"[A-Za-z0-9\.]+")
                .OfType<Match>()
                .Select(m => m.Value);

            return words.Aggregate((a, b) => a + (b.StartsWith(".") ? b : "-" + b));
        }
    }

    class webification_utility_tests
    {
        [TestCase("two little words", Result = "two-little-words")]
        [TestCase("Combined_P_A_Matrix)data_sources.csv", Result = "Combined-P-A-Matrix-data-sources.csv")]
        [TestCase("Three Little words!-v2.txt", Result = "Three-Little-words-v2.txt")]
        [TestCase("New-York NEW-YORK!.txt", Result = "New-York-NEW-YORK.txt")]
        [TestCase("my.file.txt", Result = "my.file.txt")]
        public string test_to_url_friendly_string(string s)
        {
            return WebificationUtility.ToUrlFriendlyString(s);
        }
    }
}
