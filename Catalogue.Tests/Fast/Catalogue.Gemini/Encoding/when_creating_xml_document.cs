using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Catalogue.Gemini.Encoding;
using Catalogue.Gemini.Templates;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Fast.Catalogue.Gemini.Encoding
{
    class when_creating_xml_document
    {
        [Test]
        public void should_have_correct_root_element()
        {
            var metadata = Library.Blank();
            var xml = new XmlEncoder().Create(metadata);

            string root = XName.Get("MD_Metadata", "http://www.isotc211.org/2005/gmd").ToString();
            xml.Should().HaveRoot(root);
        }
    }
}
