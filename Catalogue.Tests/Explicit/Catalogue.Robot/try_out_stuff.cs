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
            var html = XElement.Parse(indexHtmlDoc);
            var body = html.Element("body");
            body.Add(new XElement("a", new XAttribute("href", "some-topcat-record.xml")));

            body.Elements("a").Count().Should().Be(1);
        }

        private string indexHtmlDoc = @"<!DOCTYPE html>
            <html lang=""en"">
            <head>
            <title>The HTML5 Herald</title>
            <meta name = ""description"" content= ""The HTML5 Herald"" />
            <link rel= ""stylesheet"" href= ""css/styles.css"" />
            </head>
            <body>
            <script src= ""js/scripts.js"" ></script>
            </body>
            </html>";
    }
}
