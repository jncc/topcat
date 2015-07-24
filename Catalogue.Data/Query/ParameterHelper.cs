using System;
using System.Collections.Generic;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Data.Query
{
    public static class ParameterHelper
    {
        /// <summary>
        /// Parses keywords from the format we use in URL parameters.
        /// </summary>
        public static IEnumerable<MetadataKeyword> ParseMetadataKeywords(IEnumerable<string> keywords)
        {
            foreach (string s in keywords)
            {
                int lastIndexOfSlash = s.LastIndexOf('/');

                if (lastIndexOfSlash == -1)
                    yield return new MetadataKeyword { Vocab = String.Empty, Value = s.Trim() };
                else
                    yield return new MetadataKeyword
                        {
                            Vocab = "http://" + s.Substring(0, lastIndexOfSlash).Trim(),
                            Value = s.Substring(lastIndexOfSlash + 1).Trim()
                        };
            }
        }
    }

    class parameter_helper_tests
    {
        static readonly object[] KeywordTestCases =
        {
            new object[] { "", "", "" },
            new object[] { "Seabed Habitat Maps", "", "Seabed Habitat Maps" },
            new object[] { "vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps", "http://vocab.jncc.gov.uk/jncc-category", "Seabed Habitat Maps" },
            new object[] { "vocab.jncc.gov.uk/some-vocab/Maps", "http://vocab.jncc.gov.uk/some-vocab", "Maps" },
            new object[] { "vocab.jncc.gov.uk/some-vocab/Maps ", "http://vocab.jncc.gov.uk/some-vocab", "Maps" }, // space removed
            new object[] { "vocab.jncc.gov.uk/some-vocab/Maps (brackets)", "http://vocab.jncc.gov.uk/some-vocab", "Maps (brackets)" },
        };

        [Test, TestCaseSource("KeywordTestCases")]
        public void should_parse_keywords_as_expected(string input, string expectedVocab, string expectedValue)
        {
            var output = ParameterHelper.ParseMetadataKeywords(new[] { input });
            output.Should().Contain(x => x.Vocab == expectedVocab && x.Value == expectedValue);
        }
    }

}
