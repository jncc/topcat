using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using CsvHelper.Configuration;
using FluentAssertions;
using NUnit.Framework;

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

                Map(m => m.Path).Name("JNCC Location");
                Map(m => m.TopCopy).ConvertUsing(row => true); // all activities data is top copy
                Map(m => m.Status).ConvertUsing(row => Status.Publishable); // all activities data is publishable
                Map(m => m.Notes).Name("JNCC Notes");

                References<GeminiMap>(m => m.Gemini);
            }
        }

        public class GeminiMap : CsvClassMap<Metadata>
        {
            public override void CreateMap()
            {
                Map(m => m.Title).Name("Resource Title");
                Map(m => m.Abstract).Name("Resource Abstract");
                Map(m => m.TopicCategory)
                    .ConvertUsing(row => row.GetField("Topic Category")
                        .FirstCharToLower()); // correct capitalisation

                Map(m => m.Keywords).ConvertUsing(row =>
                {
                    string input = row.GetField("Keyword");
                    return ParseKeywords(input);
                });

//                Map(m => m.TemporalExtent).ConvertUsing(row =>
//                {
//                    string beg = row.GetField("TemporalExtentBegin");
//                    string end = row.GetField("TemporalExtentEnd");
//
//                    return new TemporalExtent { Begin = beg, End = end };
//                });
//                Map(m => m.DatasetReferenceDate);
//                Map(m => m.Lineage);
//                Map(m => m.ResourceLocator);
//                Map(m => m.AdditionalInformationSource);
//                Map(m => m.DataFormat);
//                Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
//                {
//                    string name = row.GetField("ResponsibleOrganisationName");
//                    string email = row.GetField("ResponsibleOrganisationEmail");
//                    string role = row.GetField("ResponsibleOrganisationRole");
//
//                    return new ResponsibleParty { Name = name, Email = email, Role = role };
//                });
//                Map(m => m.LimitationsOnPublicAccess);
//                Map(m => m.UseConstraints);
//                Map(m => m.SpatialReferenceSystem);
//                Map(m => m.MetadataDate);
//                Map(m => m.MetadataLanguage);
//                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
//                {
//                    string name = row.GetField("MetadataPOCName");
//                    string email = row.GetField("MetadataPOCEmail");
//                    string role = row.GetField("ResponsibleOrganisationRole");
//
//                    return new ResponsibleParty { Name = name, Email = email, Role = role };
//                });
//                Map(m => m.ResourceType);
//                Map(m => m.BoundingBox).ConvertUsing(row =>
//                {
//                    decimal north = Convert.ToDecimal(row.GetField("BBoxNorth"));
//                    decimal south = Convert.ToDecimal(row.GetField("BBoxSouth"));
//                    decimal east = Convert.ToDecimal(row.GetField("BBoxEast"));
//                    decimal west = Convert.ToDecimal(row.GetField("BBoxWest"));
//
//                    return new BoundingBox { North = north, South = south, East = east, West = west };
//                });
            }
        }

        public static List<Keyword> ParseKeywords(string input)
        {
            var keywords = (from each in input.Split(',') // keywords are separated by commas
                            select ParseKeywordHelper(each)).ToList();
            
            // add the broad category for activities (not included in the source data)
            keywords.Insert(0, new Keyword
                {
                   Vocab = "http://vocab.jncc.gov.uk/jncc-broad-category",
                   Value = "marine-human-activities"
                });

            return keywords;
        }

        static Keyword ParseKeywordHelper(string s)
        {
            var vocabAndValue = (from x in s.Trim().Split(new [] {"::"}, StringSplitOptions.None)
                                 select x.Trim()).ToList(); // vocab::value pairs are separated by two colons

            if (vocabAndValue.Count == 1) // no vocab (just a value)
            {
                return new Keyword
                    {
                        Value = vocabAndValue.Single(),
                    };
            }
            else
            {
                return new Keyword
                    {
                        Vocab = MapSourceVocabToRealVocab(vocabAndValue.ElementAt(0)),
                        Value = vocabAndValue.ElementAt(1),
                    };
            }
        }

        public static string MapSourceVocabToRealVocab(string v)
        {
            switch (v)
            {
//                case "SeabedSurveyPurpose": return "http://vocab.jncc.gov.uk/seabed-survey-purpose";
//                case "SeabedSurveyTechnique": return "http://vocab.jncc.gov.uk/seabed-survey-technique";
//                case "SeabedMapStatus": return "http://vocab.jncc.gov.uk/seabed-map-status";
//                case "OriginalSeabedClassificationSystem": return "http://vocab.jncc.gov.uk/original-seabed-classification-system";
//                case "MESH_GUI": return "http://vocab.jncc.gov.uk/mesh-gui";
//                case "reference-manager-code": return "http://vocab.jncc.gov.uk/reference-manager-code";
                default:
                    return v; // throw new Exception("Unsupported vocab " + v);
            }
        }
    }

    [Explicit] // this data isn't seed data so these tests are/were only used for the "one-off" import
    class when_importing_activities_data
    {
        List<Record> imported;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var store = new InMemoryDatabaseHelper().Create();

            using (var db = store.OpenSession())
            {
                var importer = new Importer<ActivitiesMapping>(new FileSystem(), new RecordService(db, new RecordValidator()));
                importer.SkipBadRecords = true; // todo remove this
                importer.Import(@"C:\Work\pressures-data\Human_Activities_Metadata_Catalogue.csv");
                db.SaveChanges();

                imported = db.Query<Record>()
                             .Customize(x => x.WaitForNonStaleResults())
                             .Take(1000).ToList();
            }
        }

        [Test]
        public void should_import_every_record()
        {
            imported.Count().Should().Be(95);
        }

        [Test]
        public void should_import_all_records_as_top_copy()
        {
            imported.All(r => r.TopCopy).Should().BeTrue();
        }

        [Test]
        public void should_import_all_records_as_publishable()
        {
            imported.All(r => r.Status == Status.Publishable).Should().BeTrue();
        }

        [Test]
        public void should_import_notes()
        {
            imported.Any(r => r.Notes.Contains("Further information can be found here")).Should().BeTrue();
        }

        [Test]
        public void should_import_gemini_records_for_all_records()
        {
            imported.All(r => r.Gemini != null).Should().BeTrue();
        }

        [Test]
        public void should_import_title()
        {
            imported.First().Gemini.Title.Should().Be("Marine Aggregate Application Areas");
            imported.Last().Gemini.Title.Should().Be("Human Activities Geodatabase");
        }

        [Test]
        public void should_import_abstract()
        {
            imported.Select(r => r.Gemini.Abstract)
                .Should().Contain(a => a != null && a.Contains("This dataset displays the location of marinas"));
        }

        [Test]
        public void should_import_topic_category()
        {
            imported.Count(r => r.Gemini.TopicCategory == "utilitiesCommunication").Should().BeGreaterThan(3);
            imported.Count(r => r.Gemini.TopicCategory == "transportation").Should().BeGreaterThan(3);
            imported.Count(r => r.Gemini.TopicCategory == "structure").Should().BeGreaterThan(3);
        }

        [Test]
        public void should_import_broad_category_keyword()
        {
            // activities data is categorised as 'marine-human-activities'
            imported.Count(r => r.Gemini.Keywords
                .Any(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "marine-human-activities"))
                .Should().Be(93);
        }

        [Test]
        public void should_import_keywords()
        {
            imported.Count(r => r.Gemini.Keywords
                .Any(k => k.Vocab == "Wiki List" && k.Value == "Marina"))
                .Should().BeGreaterThan(1);
        }

        [Test]
        public void should_import_keywords_that_have_no_vocab_namespace()
        {
            imported.Count(r => r.Gemini.Keywords
                .Any(k => k.Vocab.IsBlank() && k.Value == "Extraction"))
                .Should().BeGreaterThan(1);
        }


        [Test]
        public void source_identifiers_should_be_unique()
        {
//            imported.Select(r => r.SourceIdentifier).Distinct().Count().Should().Be(189);
        }

        [Test]
        public void all_records_should_have_a_valid_path()
        {
//            Uri uri; // need this for Uri.TryCreate; not actually using it
//
//            imported.Count(r => Uri.TryCreate(r.Path, UriKind.Absolute, out uri))
//                    .Should().Be(189);
        }

        [Test]
        public void should_import_data_format()
        {
//            imported.Count(r => r.Gemini.DataFormat == "Geographic Information System").Should().BeGreaterThan(100);
        }
    }


}
