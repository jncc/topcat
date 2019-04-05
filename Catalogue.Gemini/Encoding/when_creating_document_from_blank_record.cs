using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Gemini.Encoding
{
    class when_creating_document_from_blank_record
    {
        [Test]
        public void should_have_correct_root_element()
        {
            var metadata = Library.Blank();
            var id = Guid.NewGuid().ToString();
            var xml = new XmlEncoder().Create(id, metadata, new List<OnlineResource>());

            var root = XName.Get("MD_Metadata", "http://www.isotc211.org/2005/gmd");
            xml.Should().HaveRoot(root);
        }
    }
}
