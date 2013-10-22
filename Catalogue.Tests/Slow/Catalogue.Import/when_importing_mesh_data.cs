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
            imported.Count(r => r.Gemini.Keywords
                .Any(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category"))
                .Should().Be(191);
        }

        [Test]
        public void should_import_topic_category()
        {
            // all records are "geoscientificInformation"
            imported.Count(r => r.Gemini.TopicCategory == "geoscientificInformation").Should().BeGreaterThan(100);
        }
    }
}
