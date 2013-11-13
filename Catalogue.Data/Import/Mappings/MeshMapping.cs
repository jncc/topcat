using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using CsvHelper.Configuration;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Data.Import.Mappings
{
    /// <summary>
    /// The mappings for importing the Marine Habitat (MESH) data.
    /// </summary>
    public class MeshMapping : IMapping
    {
        public void Apply(CsvConfiguration config)
        {
            // see http://joshclose.github.io/CsvHelper/

            config.TrimFields = true;
            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<GeminiMap>();
        }

        public class RecordMap : CsvClassMap<Record>
        {
            public override void CreateMap()
            {

                Map(m => m.Path);
                Map(m => m.TopCopy).ConvertUsing(row => true); // all mesh data is top copy
                Map(m => m.SourceIdentifier).Name("AlternateTitle");

                References<GeminiMap>(m => m.Gemini);
            }
        }

        public class GeminiMap : CsvClassMap<Metadata>
        {
            public override void CreateMap()
            {
                Map(m => m.Title);
                Map(m => m.Abstract);
                Map(m => m.TopicCategory);
                Map(m => m.Keywords).ConvertUsing(row =>
                    {
                        string input = row.GetField("Keywords");
                        return ParseMeshKeywords(input);
                    });
                Map(m => m.TemporalExtent).ConvertUsing(row =>
                    {
                        string beg = row.GetField("TemporalExtentBegin");
                        string end = row.GetField("TemporalExtentEnd");

                        return new TemporalExtent { Begin = beg, End = end };
                    });
                Map(m => m.DatasetReferenceDate);
                Map(m => m.Lineage);
                Map(m => m.ResourceLocator);
                Map(m => m.AdditionalInformationSource);
                Map(m => m.DataFormat);
                Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
                    {
                        string name = row.GetField("ResponsibleOrganisationName");
                        string email = RemoveAntiSpamCharacter(row.GetField("ResponsibleOrganisationEmail"));
                        string role = row.GetField("ResponsibleOrganisationRole");

                        return new ResponsibleParty { Name = name, Email = email, Role = role };
                    });
                Map(m => m.LimitationsOnPublicAccess);
                Map(m => m.UseConstraints);
                Map(m => m.SpatialReferenceSystem);
                Map(m => m.MetadataDate);
                Map(m => m.MetadataLanguage);
                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
                {
                    string name = row.GetField("MetadataPOCName");
                    string email = RemoveAntiSpamCharacter(row.GetField("MetadataPOCEmail"));
                    string role = row.GetField("ResponsibleOrganisationRole");

                    return new ResponsibleParty { Name = name, Email = email, Role = role };
                });
                Map(m => m.ResourceType);
                Map(m => m.BoundingBox).ConvertUsing(row =>
                    {
                        decimal north = Convert.ToDecimal(row.GetField("BBoxNorth"));
                        decimal south = Convert.ToDecimal(row.GetField("BBoxSouth"));
                        decimal east = Decimal.Parse(row.GetField("BBoxEast"), NumberStyles.Float);
                        decimal west = Convert.ToDecimal(row.GetField("BBoxWest"));

                        return new BoundingBox { North = north, South = south, East = east, West = west };
                    });
            }

            string RemoveAntiSpamCharacter(string email)
            {
                // remove the anti-spam prepended 'x'
                if (email.IsNotBlank() && email.StartsWith("x"))
                    return email.Remove(0, 1);
                else
                    return email;
            }
        }

        public static List<Keyword> ParseMeshKeywords(string input)
        {
            var q = from m in Regex.Matches(input, @"\{(.*?)\}").Cast<Match>()
                    let pair = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1).First().Split(',')
                    let vocab = pair.ElementAt(0).Trim().Trim('"').Trim()
                    let keyword = pair.ElementAt(1).Trim().Trim('"').Trim()
                    select new Keyword
                    {
                        // todo: map the source vocab IDs to "real" ones
                        Vocab = MapSourceVocabToRealVocab(vocab),
                        Value = MapSourceKeywordToRealKeyword(keyword),
                    };

            return q.ToList();
        }

        public static string MapSourceVocabToRealVocab(string v)
        {
            switch (v)
            {
                case "jncc-broad-category": return "http://vocab.jncc.gov.uk/jncc-broad-category";
                case "SeabedSurveyPurpose": return "http://vocab.jncc.gov.uk/seabed-survey-purpose";
                case "SeabedSurveyTechnique": return "http://vocab.jncc.gov.uk/seabed-survey-technique";
                case "SeabedMapStatus": return "http://vocab.jncc.gov.uk/seabed-map-status";
                case "OriginalSeabedClassificationSystem": return "http://vocab.jncc.gov.uk/original-seabed-classification-system";
                case "MESH_GUI": return "http://vocab.jncc.gov.uk/mesh-gui";
                default: throw new Exception("Unsupported vocab " + v);
            }
        }

        public static string MapSourceKeywordToRealKeyword(string w)
        {
            switch (w)
            {
                case "SeabedHabitatMaps": return "seabed-habitat-maps";
                default: return w;
            }
        }
    }


    class when_importing_mesh_data
    {
        [Test]
        public void should_be_able_to_parse_keywords()
        {
            // a string from the raw mesh data file
            string input = "{\"jncc-broad-category\", \"SeabedHabitatMaps\"}, {\"OriginalSeabedClassificationSystem\", \"Local\"}, {\"SeabedMapStatus\", \"Show on webGIS\"}, {\"SeabedMapStatus\", \"Translated to EUNIS\"}, {\"SeabedMapStatus\", \"Data Provider Agreement signed FULL ACCESS\"}";

            var keywords = MeshMapping.ParseMeshKeywords(input);

            keywords.Should().HaveCount(5);
            keywords.Select(k => k.Vocab).Should().ContainInOrder(new[]
                {
                    "http://vocab.jncc.gov.uk/jncc-broad-category",
                    "http://vocab.jncc.gov.uk/original-seabed-classification-system",
                    "http://vocab.jncc.gov.uk/seabed-map-status",
                    "http://vocab.jncc.gov.uk/seabed-map-status",
                    "http://vocab.jncc.gov.uk/seabed-map-status",
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
