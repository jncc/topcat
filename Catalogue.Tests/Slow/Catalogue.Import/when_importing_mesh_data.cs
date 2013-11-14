using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Tests.Utility;
using Catalogue.Utilities.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Import
{
    class when_importing_mesh_data : DatabaseTestFixture
    {
        List<Record> imported;

        [SetUp]
        public void SetUp()
        {
            // the DatabaseTestFixture will already have done the import ..!
            // let's store everything in a list to allow standard linq-to-object queries
            // (all mesh records have a "GUI" field which seems to be unique and starts with 'GB')
            imported = Db.Query<Record>().Take(1000).ToList()
                .Where(r => r.SourceIdentifier.IsNotBlank() && r.SourceIdentifier.StartsWith("GB"))
                .ToList();
        }

        [Test]
        public void should_import_all_mesh_data()
        {
            imported.Count().Should().Be(189);
        }

        [Test]
        public void should_import_keywords()
        {
            // mesh data is categorised as 'seabed-habitat-maps'
            imported.Count(r => r.Gemini.Keywords
                .Any(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "seabed-habitat-maps"))
                .Should().Be(189);
        }

        [Test]
        public void should_import_topic_category()
        {
            imported.Count(r => r.Gemini.TopicCategory == "geoscientificInformation").Should().Be(6);
            imported.Count(r => r.Gemini.TopicCategory == "biota").Should().Be(182);
            imported.Count(r => r.Gemini.TopicCategory == "environment").Should().Be(1);

            (6 + 182 + 1).Should().Be(189); // lolz
        }

        [Test]
        public void source_identifiers_should_be_unique()
        {
            imported.Select(r => r.SourceIdentifier).Distinct().Count().Should().Be(189);
        }

        [Test]
        public void all_records_should_have_a_valid_path()
        {
            Uri uri; // need this for Uri.TryCreate; not actually using it

            imported.Count(r => Uri.TryCreate(r.Path, UriKind.Absolute, out uri))
                    .Should().Be(189);
        }

        [Test]
        public void all_records_should_be_marked_as_top_copy()
        {
            imported.Count(r => r.TopCopy).Should().Be(189);
        }

        [Test]
        public void should_import_data_format()
        {
            imported.Count(r => r.Gemini.DataFormat == "Geographic Information System").Should().BeGreaterThan(100);
        }
    }
}
