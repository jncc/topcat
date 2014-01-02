using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using CsvHelper.Configuration;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Document;

namespace Catalogue.Data.Import.Mappings
{
    public class ActivitiesMapping : IMapping
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
                Map(m => m.Status).ConvertUsing(row => Status.Publishable); // all mesh data is publishable
                Map(m => m.SourceIdentifier).Name("AlternateTitle"); // the mesh "GUI"
                Map(m => m.Notes);

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
                    string email = row.GetField("ResponsibleOrganisationEmail");
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
                    string email = row.GetField("MetadataPOCEmail");
                    string role = row.GetField("ResponsibleOrganisationRole");

                    return new ResponsibleParty { Name = name, Email = email, Role = role };
                });
                Map(m => m.ResourceType);
                Map(m => m.BoundingBox).ConvertUsing(row =>
                {
                    decimal north = Convert.ToDecimal(row.GetField("BBoxNorth"));
                    decimal south = Convert.ToDecimal(row.GetField("BBoxSouth"));
                    decimal east = Convert.ToDecimal(row.GetField("BBoxEast"));
                    decimal west = Convert.ToDecimal(row.GetField("BBoxWest"));

                    return new BoundingBox { North = north, South = south, East = east, West = west };
                });
            }
        }

        public static List<Keyword> ParseMeshKeywords(string input)
        {
            var q = from m in Regex.Matches(input, @"\{(.*?)\}").Cast<Match>()
                    let pair = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1).First().Split(',')
                    let vocab = pair.ElementAt(0).Trim().Trim('"').Trim()
                    let keyword = pair.ElementAt(1).Trim().Trim('"').Trim()
                    where keyword.IsNotBlank()
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
//                case "jncc-broad-category": return "http://vocab.jncc.gov.uk/jncc-broad-category";
//                case "SeabedSurveyPurpose": return "http://vocab.jncc.gov.uk/seabed-survey-purpose";
//                case "SeabedSurveyTechnique": return "http://vocab.jncc.gov.uk/seabed-survey-technique";
//                case "SeabedMapStatus": return "http://vocab.jncc.gov.uk/seabed-map-status";
//                case "OriginalSeabedClassificationSystem": return "http://vocab.jncc.gov.uk/original-seabed-classification-system";
//                case "MESH_GUI": return "http://vocab.jncc.gov.uk/mesh-gui";
//                case "reference-manager-code": return "http://vocab.jncc.gov.uk/reference-manager-code";
                default: throw new Exception("Unsupported vocab " + v);
            }
        }

        public static string MapSourceKeywordToRealKeyword(string w)
        {
            switch (w)
            {
//                case "SeabedHabitatMaps": return "Seabed-Habitat-Maps";
                default: return w;
            }
        }

    }

    class activities_importer
    {
        [Explicit]
        [Test]
        public void run()
        {
            var store = new DocumentStore();
            store.ParseConnectionString("Url=http://localhost:8888/");
            store.Initialize();

            using (var db = store.OpenSession())
            {
                var importer = new Importer<ActivitiesMapping>(new FileSystem(), new RecordService(db, new RecordValidator()));
                importer.Import(@"C:\Work\pressures-data\Human_Activities_Metadata_Catalogue.csv");
            }
        }
    }

//    [Explicit] // this data isn't seed data so these tests are/were only used for the "one-off" import
//    class when_importing_activities_data
//    {
//        List<Record> imported;
//
//        [SetUp]
//        public void SetUp()
//        {
//            // the DatabaseTestFixture will already have done the import ..!
//            // let's store everything in a list to allow standard linq-to-object queries
//            // (all mesh records have a "GUI" field which seems to be unique and starts with 'GB')
//            imported = Db.Query<Record>().Take(1000).ToList()
//                .Where(r => r.SourceIdentifier.IsNotBlank() && r.SourceIdentifier.StartsWith("GB"))
//                .ToList();
//        }
//
//        [Test]
//        public void should_import_all_mesh_data()
//        {
//            imported.Count().Should().Be(189);
//        }
//
//        [Test]
//        public void should_import_keywords()
//        {
//            // mesh data is categorised as 'Seabed-Habitat-Maps'
//            imported.Count(r => r.Gemini.Keywords
//                .Any(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "Seabed-Habitat-Maps"))
//                .Should().Be(189);
//        }
//
//        [Test]
//        public void should_import_topic_category()
//        {
//            imported.Count(r => r.Gemini.TopicCategory == "geoscientificInformation").Should().Be(6);
//            imported.Count(r => r.Gemini.TopicCategory == "biota").Should().Be(182);
//            imported.Count(r => r.Gemini.TopicCategory == "environment").Should().Be(1);
//
//            (6 + 182 + 1).Should().Be(189); // lolz
//        }
//
//        [Test]
//        public void source_identifiers_should_be_unique()
//        {
//            imported.Select(r => r.SourceIdentifier).Distinct().Count().Should().Be(189);
//        }
//
//        [Test]
//        public void all_records_should_have_a_valid_path()
//        {
//            Uri uri; // need this for Uri.TryCreate; not actually using it
//
//            imported.Count(r => Uri.TryCreate(r.Path, UriKind.Absolute, out uri))
//                    .Should().Be(189);
//        }
//
//        [Test]
//        public void all_records_should_be_marked_as_top_copy()
//        {
//            imported.Count(r => r.TopCopy).Should().Be(189);
//        }
//
//        [Test]
//        public void should_import_data_format()
//        {
//            imported.Count(r => r.Gemini.DataFormat == "Geographic Information System").Should().BeGreaterThan(100);
//        }
//    }


}
