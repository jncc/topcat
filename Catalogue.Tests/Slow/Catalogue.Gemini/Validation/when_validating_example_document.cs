using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Encoding;
using Catalogue.Gemini.Templates;
using Catalogue.Gemini.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Gemini.Validation
{
    /// <summary>
    /// This is an end-to-end style test that exercises several bits of the system
    /// and depends on an external web service.
    /// </summary>
    class when_validating_example_document
    {
        [Test]
        public void should_be_valid_gemini()
        {
            var metadata = Library.Example();
            var doc = new XmlEncoder().Create(metadata);
            var result = new Validator().Validate(doc);

            string status = result.Root.Elements("result")
                .Single(e => e.Element("validation").Value == "GEMINI2.1_Schematron1.3.xsl")
                .Element("status").Value;

            status.Should().Be("valid");
        }
    }
}
