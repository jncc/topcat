using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Explicit.Catalogue.Robot
{
    class try_out_stuff
    {
        [Explicit, Test]
        public void blah()
        {
            string html = "<html><body></body></html> ";
            var doc = XElement.Parse(html);
            var body = doc.Element("body");
            body.Add(new XElement("a", new XAttribute("href", "some-topcat-record.xml")));

            body.Elements().Count().Should().Be(1);
        }
    }
}
