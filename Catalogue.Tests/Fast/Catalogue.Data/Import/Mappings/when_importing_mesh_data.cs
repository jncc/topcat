using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Catalogue.Data.Import.Mappings;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Fast.Catalogue.Data.Import.Mappings
{
    class when_importing_mesh_data
    {
        [Test]
        public void should_be_able_to_parse_keywords()
        {
            string input = "{\"jncc-broad-category\", \"SeabedHabitatMaps\"}, {\"OriginalSeabedClassificationSystem\", \"Local\"}, {\"SeabedMapStatus\", \"Show on webGIS\"}, {\"SeabedMapStatus\", \"Translated to EUNIS\"}, {\"SeabedMapStatus\", \"Data Provider Agreement signed FULL ACCESS\"}";

            var keywords = MeshMapping.ParseMeshKeywords(input);

            keywords.Should().HaveCount(5);
            keywords.Select(k => k.Vocab).Should().ContainInOrder(new []
                {
                    "jncc-broad-category",
                    "OriginalSeabedClassificationSystem",
                    "SeabedMapStatus",
                    "SeabedMapStatus",
                    "SeabedMapStatus",
                });
            keywords.Select(k => k.Value).Should().ContainInOrder(new[]
                {
                    "SeabedHabitatMaps",
                    "Local",
                    "Show on webGIS",
                    "Translated to EUNIS",
                    "Data Provider Agreement signed FULL ACCESS",
                });
        }
    }
}
