using System;
using System.Collections.Generic;
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
        [Explicit, Test]
        public void reproduce_xml_enocding_issue()
        {
            string xml = @"<some-element>© JNCC</some-element>";

            new XDocument(xml).ToString().Should().Contain("&copy;");
        }
    }
}
