using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Explicit
{
    class test_out_xml_encoding
    {
        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        [Explicit, Test]
        public void reproduce_xml_enocoding_issue()
        {
            string xml = @"<some-element>© JNCC</some-element>";
            var doc = XDocument.Parse(xml);

            doc.Declaration = new XDeclaration("1.0", "utf-8", null);
            var writer = new Utf8StringWriter();
            doc.Save(writer, SaveOptions.None);
            string metaXmlDoc = writer.ToString();

            
//            
//            string xml = @"<some-element>© JNCC</some-element>";
//            var doc = XDocument.Parse(xml);
//            doc.ToString().Should().Contain("©");
//            doc.ToString().Should().NotContain("&copy;");
        }

        [Explicit, Test]
        public void should_correctly_encode_copyright()
        {
            string xml = @"<some-element>© JNCC</some-element>";
            var doc = XDocument.Parse(xml);

            var b = new StringBuilder();
            using (TextWriter writer = new StringWriter(b))
            {
                doc.Save(writer);
            }
            b.ToString().Should().Contain("&copy;");
        }
    }
}
