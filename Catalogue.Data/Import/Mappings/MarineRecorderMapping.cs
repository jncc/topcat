using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Catalogue.Data.Import.Mappings
{
    /// <summary>
    /// Steps: unknown. Get CSV(?) from Graham.
    /// </summary>
    public class MarineRecorderMapping : IMapping
    {
        public IEnumerable<Vocabulary> RequiredVocabularies
        {
            get
            {
                return new List<Vocabulary>
                {
                    Vocabularies.JnccCategory,
                    Vocabularies.JnccDomain,
                };
            }
        }

        public void Apply(CsvConfiguration config)
        {
            config.RegisterClassMap<MarineRecorderMapping.RecordMap>();
            config.RegisterClassMap<MarineRecorderMapping.GeminiMap>();

            config.WillThrowOnMissingField = false;
            config.TrimFields = true;
        }

        public sealed class GeminiMap : CsvClassMap<Metadata>
        {
            public GeminiMap()
            {
                Map(m => m.Title).Field("Gemini.Title");
                Map(m => m.Abstract).Field("Gemini.Abstract");
                Map(m => m.TopicCategory).Field("Gemini.TopicCategory", value => value.FirstCharToLower());
                Map(m => m.Keywords).ConvertUsing(row =>
                {
                    string surveyId = row.GetField("Gemini.Keywords.SurveyKey");
                    string surveyType = row.GetField("Geminin.Keywords.SurveyType");

                    var keywords = new List<MetadataKeyword>
                    {
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine"},
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Marine Recorder"},
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/survey-id", Value = surveyId },
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/survey-type", Value = surveyType },
                    };

                    return keywords;
                });
                Map(m => m.TemporalExtent).ConvertUsing(row => new TemporalExtent
                {
                    Begin = row.GetField("TemporalExtent.Begin"),
                    End = row.GetField("TemporalExtent.End")
                });
                Map(m => m.DatasetReferenceDate).Field("Gemini.DatasetReferenceDate");
                Map(m => m.Lineage).Field("Gemini.Lineage");
                Map(m => m.ResourceLocator).Ignore();
                Map(m => m.AdditionalInformationSource).Field("Gemini.AdditionalInformationSource");
                Map(m => m.DataFormat).Field("Gemini.DataFormat");
                Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
                {
                    string name = row.GetField("ResponsibleOrganisation.Name").Trim();
                    string email = row.GetField("ResponsibleOrganisation.Email").Trim();
                    string role = row.GetField("ResponsibleOrganisation.Role").FirstCharToLower().Trim();

                    return new ResponsibleParty { Name = name == "JNCC" ? "Joint Nature Conservation Committee (JNCC)" : name, Email = email, Role = role };
                });
                Map(m => m.LimitationsOnPublicAccess).Field("Gemini.LimitationsOnPublicAccess");
                Map(m => m.UseConstraints).Field("Gemini.UseConstraints");
                Map(m => m.SpatialReferenceSystem).Field("Gemini.SpatialReferenceSystem", value => value == "N/A" ? null : value);
                Map(m => m.Extent).Ignore();
                Map(m => m.MetadataDate).Value(DateTime.Now);
                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
                {
                    string name = "Joint Nature Conservation Committee (JNCC)";
                    string email = "data@jncc.gov.uk";
                    string role = "pointOfContact";
                    return new ResponsibleParty { Name = name, Email = email, Role = role };
                });
                Map(m => m.ResourceType).Field("Gemini.ResourceType", value => value.FirstCharToLower());
                Map(m => m.BoundingBox).ConvertUsing(row =>
                {
                    decimal north = Convert.ToDecimal(row.GetField("BoundingBox.North"));
                    decimal south = Convert.ToDecimal(row.GetField("BoundingBox.South"));
                    decimal east = Convert.ToDecimal(row.GetField("BoundingBox.East"));
                    decimal west = Convert.ToDecimal(row.GetField("BoundingBox.West"));

                    return new BoundingBox { North = north, South = south, East = east, West = west };
                });
            }
        }

        public sealed class RecordMap : CsvClassMap<Record>
        {
            public RecordMap()
            {
                Map(m => m.Path);
                Map(m => m.TopCopy);
                Map(m => m.Validation).Value(Validation.Gemini);
                Map(m => m.Status).Ignore();
                Map(m => m.Security).Ignore();
                Map(m => m.Review).Ignore();
                Map(m => m.Notes);
                Map(m => m.SourceIdentifier);
                Map(m => m.ReadOnly).Value(true);

                References<MarineRecorderMapping.GeminiMap>(m => m.Gemini);
            }
        }
    }

    class when_importing_marine_recorder_dump
    {
        List<Record> imported;

        [TestFixtureSetUp]
        public void SetUp()
        {
//          var paths = new { input = @"C:\work\marine-recorder-dump\biotopeMetadata.csv", errors = @"C:\work\marine-recorder-dump\biotope-errors.txt" };
            var paths = new { input = @"C:\work\marine-recorder-dump\speciesMetadata.csv", errors = @"C:\work\marine-recorder-dump\species-errors.txt" };

            var store = new InMemoryDatabaseHelper().Create();

            using (var db = store.OpenSession())
            {
                try
                {
                    var importer = Importer.CreateImporter<MarineRecorderMapping>(db);
                    importer.SkipBadRecords = true;
                    importer.Import(paths.input);

                    var errors = importer.Results
                        .Where(r => !r.Success)
                        .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                    File.WriteAllLines(paths.errors, errors);

                    db.SaveChanges();

                    imported = db.Query<Record>()
                                 .Customize(x => x.WaitForNonStaleResults())
                                 .Take(1000).ToList();

                }
                catch (CsvHelperException ex)
                {
                    string s = (string)ex.Data["CsvHelper"];
                    throw;
                }
            }
        }

        [Test, Explicit] // this isn't seed data, so these tests are (were) only used for the "one-off" import
        public void should_import_expected_number_of_records()
        {
            imported.Count().Should().Be(247);
        }
    }
}
