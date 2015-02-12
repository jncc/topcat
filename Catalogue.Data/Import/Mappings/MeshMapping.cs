﻿using System;
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
    ///     The mappings for importing the Marine Habitat (MESH) data.
    /// </summary>
    public class MeshMapping : IMapping
    {
        public IEnumerable<Vocabulary> Vocabularies { get; private set; }

        public MeshMapping()
        {
            //Vocabs are provided in the seeder.
            Vocabularies = new List<Vocabulary>();
        }

        public void Apply(CsvConfiguration config)
        {
            // see http://joshclose.github.io/CsvHelper/

            config.TrimFields = true;
            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<GeminiMap>();
        }

        public static List<MetadataKeyword> ParseMeshKeywords(string input)
        {
            IEnumerable<MetadataKeyword> q = from m in Regex.Matches(input, @"\{(.*?)\}").Cast<Match>()
                let pair = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1).First().Split(',')
                let vocab = pair.ElementAt(0).Trim().Trim('"').Trim()
                let keyword = pair.ElementAt(1).Trim().Trim('"').Trim()
                where keyword.IsNotBlank()
                select new MetadataKeyword
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
                case "jncc-broad-category":
                    return "http://vocab.jncc.gov.uk/jncc-broad-category";
                case "SeabedSurveyPurpose":
                    return "http://vocab.jncc.gov.uk/seabed-survey-purpose";
                case "SeabedSurveyTechnique":
                    return "http://vocab.jncc.gov.uk/seabed-survey-technique";
                case "SeabedMapStatus":
                    return "http://vocab.jncc.gov.uk/seabed-map-status";
                case "OriginalSeabedClassificationSystem":
                    return "http://vocab.jncc.gov.uk/original-seabed-classification-system";
                case "MESH_GUI":
                    return "http://vocab.jncc.gov.uk/mesh-gui";
                case "reference-manager-code":
                    return "http://vocab.jncc.gov.uk/reference-manager-code";
                default:
                    throw new Exception("Unsupported vocab " + v);
            }
        }

        public static string MapSourceKeywordToRealKeyword(string w)
        {
            switch (w)
            {
                case "SeabedHabitatMaps":
                    return "Seabed Habitat Maps";
                default:
                    return w;
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
                    string begin =  row.GetField("TemporalExtentBegin");
                    string end = row.GetField("TemporalExtentEnd");
                    return new TemporalExtent
                        {
                            Begin = ImportUtility.ParseDate(begin),
                            End = ImportUtility.ParseDate(end)
                        };
                });
                Map(m => m.DatasetReferenceDate).ConvertUsing(row =>
                    {
                        return ImportUtility.ParseDate(row.GetField("DatasetReferenceDate"));
                    });
                Map(m => m.Lineage);
                Map(m => m.ResourceLocator);
                Map(m => m.AdditionalInformationSource);
                Map(m => m.DataFormat);
                Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
                {
                    string name = row.GetField("ResponsibleOrganisationName");
                    string email = RemoveAntiSpamCharacter(row.GetField("ResponsibleOrganisationEmail"));
                    string role = row.GetField("ResponsibleOrganisationRole");

                    return new ResponsibleParty {Name = name, Email = email, Role = role};
                });
                Map(m => m.LimitationsOnPublicAccess);
                Map(m => m.UseConstraints);
                Map(m => m.SpatialReferenceSystem);
                Map(m => m.MetadataDate).ConvertUsing(row =>
                {
                    return DateTime.Parse(row.GetField("MetadataDate"));
                    
                });
                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
                {
                    string name = row.GetField("MetadataPOCName");
                    string email = RemoveAntiSpamCharacter(row.GetField("MetadataPOCEmail"));
                    string role = row.GetField("ResponsibleOrganisationRole");

                    return new ResponsibleParty {Name = name, Email = email, Role = role};
                });
                Map(m => m.ResourceType);
                Map(m => m.BoundingBox).ConvertUsing(row =>
                {
                    decimal north = Convert.ToDecimal(row.GetField("BBoxNorth"));
                    decimal south = Convert.ToDecimal(row.GetField("BBoxSouth"));
                    decimal east = Convert.ToDecimal(row.GetField("BBoxEast"));
                    decimal west = Convert.ToDecimal(row.GetField("BBoxWest"));

                    return new BoundingBox {North = north, South = south, East = east, West = west};
                });
            }

            private string RemoveAntiSpamCharacter(string email)
            {
                // remove the anti-spam prepended 'x'
                if (email.IsNotBlank() && email.StartsWith("x"))
                    return email.Remove(0, 1);
                return email;
            }
        }

        public class RecordMap : CsvClassMap<Record>
        {
            public override void CreateMap()
            {
                Map(m => m.Path);
                Map(m => m.TopCopy).ConvertUsing(row => true); // all mesh data is top copy
                Map(m => m.Status).ConvertUsing(row => Status.Publishable); // all mesh data is publishable
                Map(m => m.SourceIdentifier).Name("AlternateTitle"); // the mesh "GUI"
                Map(m => m.Notes);

                References<GeminiMap>(m => m.Gemini);
            }
        }
    }


    internal class when_importing_mesh_data
    {
        [Test]
        public void should_be_able_to_parse_keywords()
        {
            // a string from the raw mesh data file
            string input =
                "{\"jncc-broad-category\", \"SeabedHabitatMaps\"}, {\"OriginalSeabedClassificationSystem\", \"Local\"}, {\"SeabedMapStatus\", \"Show on webGIS\"}, {\"SeabedMapStatus\", \"Translated to EUNIS\"}, {\"SeabedMapStatus\", \"Data Provider Agreement signed FULL ACCESS\"}";

            List<MetadataKeyword> keywords = MeshMapping.ParseMeshKeywords(input);

            keywords.Should().HaveCount(5);
            keywords.Select(k => k.Vocab).Should().ContainInOrder(new[]
            {
                "http://vocab.jncc.gov.uk/jncc-broad-category",
                "http://vocab.jncc.gov.uk/original-seabed-classification-system",
                "http://vocab.jncc.gov.uk/seabed-map-status",
                "http://vocab.jncc.gov.uk/seabed-map-status",
                "http://vocab.jncc.gov.uk/seabed-map-status"
            });
            keywords.Select(k => k.Value).Should().ContainInOrder(new[]
            {
                "Seabed Habitat Maps",
                "Local",
                "Show on webGIS",
                "Translated to EUNIS",
                "Data Provider Agreement signed FULL ACCESS"
            });
        }
    }
}