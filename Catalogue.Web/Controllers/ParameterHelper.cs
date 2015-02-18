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
            List<MetadataKeyword> result = new List<MetadataKeyword>();

            var k1 = (from k in keywords
                        where k.IsNotBlank() && k.IndexOf('/') == -1
                        select new MetadataKeyword
                            {
                                Vocab = String.Empty,
                                Value = k
                            }).ToList();

            result.AddRange(k1);

            var k2 =  (from k in keywords
                    where k.IsNotBlank()
                    from m in Regex.Matches(k, @"^([\w\s/\.-]*)/([\w\s-]*)$", RegexOptions.IgnoreCase).Cast<Match>()
                    let pair = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1)
                    select new MetadataKeyword
                    {
                        Vocab = "http://" + pair.ElementAt(0).Trim(),
                        Value = pair.ElementAt(1).Trim()
                    }
                   ).ToList();

            result.AddRange(k2);

            return result;
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

        [Test]
        public void should_parse_keywords_with_no_vocab()
        {
            string urlKeyword = "Seabed Habitat Maps";

            var results = ParameterHelper.ParseKeywords(new [] { urlKeyword });

            results.Should().HaveCount(1);
        }


        [Test]
        public void should_handle_empty_set_nicely()
        {
            string urlKeyword = String.Empty;

            var results = ParameterHelper.ParseKeywords(new[] { urlKeyword });

            results.Should().HaveCount(0);
        }
    }
}