using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Utilities.Html;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Fast.Catalogue.Utilities.Html
{
    class linky_tests
    {

        string inputWithLinks = "Some text with some links such as http://angularjs.org, and ftp://127.0.0.1/.";

        [Test]
        public void should_linkify()
        {
            string expected = "Some text with some links such as " +
                              "<a href=\"http://angularjs.org\" target=\"_blank\">http://angularjs.org</a>, " +
                              "and <a href=\"ftp://127.0.0.1/\" target=\"_blank\">ftp://127.0.0.1/</a>.";

            Linky.Linkify(inputWithLinks).Should().Be(expected);
        }


    }
}
