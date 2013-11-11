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
            imported = Db.Query<Record>().Take(200).ToList()
                .Where(r => r.SourceIdentifier.IsNotBlank() && r.SourceIdentifier.StartsWith("GB"))
                .ToList();
        }

        [Test]
        public void should_import_all_mesh_data()
        {
            imported.Count().Should().Be(183);
        }

        [Test]
        public void should_import_keywords()
        {
            // mesh data is categorised as 'SeabedHabitatMaps'
            imported.Count(r => r.Gemini.Keywords
                .Any(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "SeabedHabitatMaps"))
                .Should().Be(183);
        }

        [Test]
        public void should_import_topic_category()
        {
            // all mesh records are "geoscientificInformation"
            imported.Count(r => r.Gemini.TopicCategory == "geoscientificInformation").Should().Be(183);
        }

        [Test]
        public void should_import_unique_source_identifiers()
        {
            imported.Select(r => r.SourceIdentifier).Distinct().Count().Should().Be(183);
        }

        [Test]
        public void all_records_should_have_a_valid_path()
        {
            Uri uri; // need this for Uri.TryCreate; not actually using it

            imported.Count(r => Uri.TryCreate(r.Path, UriKind.Absolute, out uri))
                    .Should().Be(183);
        }

        [Test] public void all_records_should_be_marked_as_top_copy()
        {
            imported.Count(r => r.TopCopy).Should().Be(183);
        }
    }
}
