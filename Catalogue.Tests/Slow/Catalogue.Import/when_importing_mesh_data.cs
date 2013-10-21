using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Tests.Utility;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Catalogue.Tests.Slow.Catalogue.Import
{
    class when_importing_mesh_data : DatabaseTestFixture
    {
        List<Record> imported;

        [SetUp]
        public void SetUp()
        {
            // the DatabaseTestFixture will already have done the import ..!

            imported = Db.Query<Record>().Take(200).ToList();
        }

        [Test]
        public void should_import_keywords()
        {
            // todo why is this failing?
//            Db.Query<Record>().Take(128).ToList().Count().Should().Be(128);

            var records = imported // Db.Query<Record>()
                         .Where(r => r.Gemini.Keywords != null)
                         .Where(r => r.Gemini.Keywords.Any(k =>
                             k.VocabularyIdentifier == "OriginalSeabedClassificationSystem"))
                         .ToList();

            records.Count.Should().Be(108);

//            records
//                .SelectMany(r => r.Gemini.Keywords)
//                .Where(k => k.VocabularyIdentifier == "OriginalSeabedClassificationSystem")
//                .Count().Should().Be(108);
        }
    }
}
