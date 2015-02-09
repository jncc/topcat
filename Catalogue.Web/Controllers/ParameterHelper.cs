using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Web.Controllers
{
    public static class ParameterHelper
    {
        /// <summary>
        /// Parses keywords from the format we use in URL parameters.
        /// </summary>
        public static List<MetadataKeyword> ParseKeywords(IEnumerable<string> keywords)
        {
            return (from k in keywords
                    where k.IsNotBlank()
                    from m in Regex.Matches(k, @"^([\w\s/\.-]*)/([\w\s-]*)$", RegexOptions.IgnoreCase).Cast<Match>()
                    let pair = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1)
                    select new MetadataKeyword
                    {
                        Vocab = "http://" + pair.ElementAt(0).Trim(),
                        Value = pair.ElementAt(1).Trim()
                    }
                   ).ToList();

        }

    }

    class parameter_helper_tests
    {
        [Test]
        public void should_parse_keywords()
        {
            string urlKeyword = "vocab.jncc.gov.uk/jncc-broad-category/Seabed Habitat Maps";

            var results = ParameterHelper.ParseKeywords(new [] { urlKeyword });

            results.Should().HaveCount(1);
        }
    }
}