using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Catalogue.Gemini.Encoding;
using Catalogue.Gemini.Templates;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Fast.Catalogue.Gemini.Encoding
{
    class when_updating_the_metadata_language
    {
        [Test]
        public void should_work()
        {
            var metadata = Library.Example();
            var original = new XmlEncoder().Create(metadata);

            metadata.MetadataLanguage = "fin"; // set to finnish (probably the right code..?)
            var updated = new XmlEncoder().Update(original, metadata);
            
            updated.XPath("//*/gmd:language/gmd:LanguageCode").Value.Should().Be("fin");
        }
    }
}
