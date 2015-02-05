using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Model;
using Catalogue.Gemini.DataFormats;
using Catalogue.Utilities.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Import
{
    internal class when_importing_mesh_data : DatabaseTestFixture
    {
        private List<Record> imported;

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
            // mesh data is categorised as 'Seabed Habitat Maps'
            imported.Count(r => r.Gemini.Keywords
                .Any(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "Seabed Habitat Maps"))
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
        public void should_all_have_unique_source_identifiers()
        {
            imported.Select(r => r.SourceIdentifier).Distinct().Count().Should().Be(189);
        }

        [Test]
        public void should_all_have_a_valid_path()
        {
            Uri uri; // need this for Uri.TryCreate; not actually using it

            imported.Count(r => Uri.TryCreate(r.Path, UriKind.Absolute, out uri))
                .Should().Be(189);
        }

        [Test]
        public void should_all_be_top_copy()
        {
            Console.Write("hello");
            imported.Count(r => r.TopCopy).Should().Be(189);
        }

        [Test]
        public void should_import_data_format()
        {
            imported.Count(r => r.Gemini.DataFormat.StartsWith("Geo")).Should().BeGreaterThan(100);
        }

        [Test]
        public void should_import_only_known_data_formats()
        {
            imported.Select(r => r.Gemini.DataFormat)
                .Should().OnlyContain(x => DataFormats.Known.SelectMany(g => g.Formats).Any(f => f.Name == x));
        }
    }
}