using System;
using System.Xml.Linq;
using Catalogue.Gemini.Templates;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Gemini.Encoding
{
    class when_updating_the_metadata_language
    {
        XDocument original;
        XDocument updated;

        [SetUp]
        public void set_up()
        {
            var metadata = Library.Example();
            original = new XmlEncoder().Create(Guid.NewGuid(), metadata);

            metadata.MetadataLanguage = "fin"; // set to finnish (probably the right code..?)
            updated = new XmlEncoder().Update(original, metadata);
        }

        [Test]
        public void language_values_should_be_updated_correctly()
        {
            var e = updated.XPath("//*/gmd:language/gmd:LanguageCode");
            e.Value.Should().Be("fin");
            e.Attribute("codeListValue").Value.Should().Be("fin");
        }

        [Test]
        public void updated_document_should_be_a_new_reference()
        {
            updated.Should().NotBe(original);
        }

        [Test]
        public void original_document_should_not_be_changed()
        {
            original.XPath("//*/gmd:language/gmd:LanguageCode").Value.Should().Be("eng");
        }
    }
}
