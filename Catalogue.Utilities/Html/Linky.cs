using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
}
