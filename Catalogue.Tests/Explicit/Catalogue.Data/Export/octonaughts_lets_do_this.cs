using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Export;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Catalogue.Tests.Explicit.Catalogue.Data.Export
{
    class octonaughts_lets_do_this : DatabaseTestFixture
    {

        //[Test, Explicit]
        //public void foo()
        //{
        //    var query = Db.Query<Record>().Where(r => r.Gemini.Title == "Broadscale remote survey and mapping of sublittoral habitats and their associated biota in the Firth of Lorn: biotopes");
        //    var records = query.ToList();

        //    records.Any().Should().BeTrue();

        //    using (var writer = File.CreateText(@"c:\deleteme.txt"))
        //    {
        //        var exporter = new Exporter(writer);
        //        exporter.Export(records, writer);
        //    }
        //}

        [Test, Explicit]
        public void go()
        {
            var query = Db.Query<Record>().Where(r => r.Gemini.Title == "Broadscale remote survey and mapping of sublittoral habitats and their associated biota in the Firth of Lorn: biotopes");

            var record = query.First();

            record.Should().NotBeNull();

            record.Validation = Validation.Gemini;
            record.Gemini.LimitationsOnPublicAccess = "this is a test record so shouldn't be viewed by anyone ...";

            var validator = new RecordValidator();
            var result = validator.Validate(record);

            var xml = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(new Guid("a92a3e00-2ff6-4270-b19e-377c7d542d7c"), Library.Example());
            var ceh = new global::Catalogue.Gemini.Validation.Validator().Validate(xml);

            xml.Save(@"c:\topcat-out.xml");
        }
    }
}
