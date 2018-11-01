using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Export;
using Catalogue.Data.Model;
using Catalogue.Gemini.Templates;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Data.Import
{
    class topcat_mapping_tests : CleanDbTest
    {
        [Test]
        public void blah()
        {
            var records = new [] { GetExampleRecord() };
            var writer = new StringWriter();
            new Exporter().Export(records, writer, "\t");

            string s = writer.ToString();
            s.Should().Contain("some/path");
        }

        Record GetExampleRecord()
        {
            return new Record
            {
                Path = "some/path",
                Gemini = Library.Example(),
            };
        }
    }
}
