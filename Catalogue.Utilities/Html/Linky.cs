using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Utilities.Html
{
    public static class Linky
    {
        public static string Linkify(string html)
        {
            // regex borrowed from here https://github.com/angular/angular.js/blob/master/src/ngSanitize/filter/linky.js

            var matches = Regex.Matches(html, @"((ftp|https?)://|(mailto:)?[A-Za-z0-9._%+-]+@)\S*[^\s\.\;\,\(\)\{\}\<\>]");
            
            foreach (var match in matches.Cast<Match>())
            {
                html = html.Replace(match.Value, String.Format("<a href=\"{0}\" target=\"_blank\">{0}</a>", match));
            }

            return html;
        }
    }


    class linky_tests
    {

        string inputWithLinks = "Some text with some links such as http://angularjs.org, and ftp://127.0.0.1/.";

        [Test]
        public void should_linkify()
        {
            string expected = "Some text with some links such as " +
                              "<a href=\"http://angularjs.org\" target=\"_blank\">http://angularjs.org</a>, " +
                              "and <a href=\"ftp://127.0.0.1/\" target=\"_blank\">ftp://127.0.0.1/</a>.";

            Linky.Linkify(inputWithLinks).Should().Be(expected);
        }


    }
}
