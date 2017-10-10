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
using Catalogue.Gemini.Spatial;
using Catalogue.Utilities.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Catalogue.Data.Import.Mappings
{
    /// <summary>
    /// Steps: unknown. Get CSV from someone.
    /// </summary>
    public class SeabirdSurveyMapping : IMapping
    {
        public IEnumerable<Vocabulary> RequiredVocabularies
        {
            get
            {
                return new List<Vocabulary>
                {
                    Vocabularies.JnccCategory,
                    Vocabularies.JnccDomain,
                    new Vocabulary
                    {
                        Id = "http://vocab.jncc.gov.uk/species",
                        Name = "Species",
                        Description = "An ad-hoc, unverified partial list of common name species.",
                        Controlled = true,
                        PublicationDate = "2016",
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary
                    {
                        Id = "http://vocab.jncc.gov.uk/seabird-season",
                        Name = "Seabird Season",
                        Description = "List of seasons for seabird surveys.",
                        Controlled = true,
                        PublicationDate = "2016",
                        Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "breeding" },
                            new VocabularyKeyword { Value = "non-breeding" },
                            new VocabularyKeyword { Value = "post breeding dispersal" },
                            new VocabularyKeyword { Value = "spring migration" },
                            new VocabularyKeyword { Value = "autumn migration" },
                            new VocabularyKeyword { Value = "all" },
                        }
                    },
                };
            }
        }

        public void Apply(CsvConfiguration config)
        {
            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<GeminiMap>();

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
                    string species = row.GetField("Gemini.Keywords.Species");
                    string season = row.GetField("Gemini.Keywords.season");

                    var keywords = new List<MetadataKeyword>
                    {
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine"},
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Seabird Surveys"},
                    };

                    if (species.IsNotBlank())
                        keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/species", Value = species });

                    if (season.IsNotBlank())
                        keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/seabird-season", Value = season });

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

                Map(m => m.DataFormat).Field("Gemini.DataFormat", s => s == "ArcGIS shapefile" ? "ESRI Arc/View ShapeFile" : s);
                Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
                {
                    string name = row.GetField("ResponsibleOrganisation.Name").Trim();
                    string email = row.GetField("ResponsibleOrganisation.Email").Trim();
                    string role = row.GetField("ResponsibleOrganisation.Role").FirstCharToLower().Trim();

                    return new ResponsibleParty { Name = name == "JNCC" ? "Joint Nature Conservation Committee (JNCC)" : name, Email = email, Role = role };
                });
                Map(m => m.LimitationsOnPublicAccess).Field("Gemini.LimitationsOnPublicAccess");
                Map(m => m.UseConstraints).Field("Gemini.UseConstraints");
                Map(m => m.Copyright).Field("Copyright");
                Map(m => m.SpatialReferenceSystem).Field("Gemini.SpatialReferenceSystem", value => value == "N/A" ? null : value);
                Map(m => m.Extent).ConvertUsing(row =>
                {
                    string val = row.GetField("Gemini.Extent");
                    return new List<Extent> { new Extent { Value = val } };
                });
                Map(m => m.MetadataDate).Value(DateTime.Now);
                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
                {
//                  string name = "Joint Nature Conservation Committee (JNCC)";
//                  string email = "data@jncc.gov.uk";
//                  string role = "pointOfContact";
                    string name = row.GetField("MetadataPointOfContact.Name").Trim();
                    string email = row.GetField("MetadataPointOfContact.Email").Trim();
//                    string role = row.GetField("MetadataPointOfContact.Role").FirstCharToLower().Trim();
//                    role = role == "point of contact" ? "pointOfContact" : role;

                    return new ResponsibleParty { Name = name, Email = email, Role = "pointOfContact" };
                });
                Map(m => m.ResourceType).Field("Gemini.ResourceType", value => value.FirstCharToLower());
                Map(m => m.BoundingBox).ConvertUsing(row =>
                {
                    string north = row.GetField("BoundingBox.North");

                    if (north.IsBlank())
                        return null;
                    else
                    {
                        return new BoundingBox
                        {
                            North = Convert.ToDecimal(north),
                            South = Convert.ToDecimal(row.GetField("BoundingBox.South")),
                            East = Convert.ToDecimal(row.GetField("BoundingBox.East")),
                            West = Convert.ToDecimal(row.GetField("BoundingBox.West"))
                        };
                    }
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
                Map(m => m.Status).Value(Status.Publishable);
                Map(m => m.Security).Ignore();
                Map(m => m.Review).Ignore();
                Map(m => m.Notes);
                Map(m => m.SourceIdentifier);
                Map(m => m.ReadOnly);

                References<GeminiMap>(m => m.Gemini);
            }
        }
    }

    [Explicit]
    class when_importing_seabird_survey_records
    {
        List<Record> imported;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var store = new InMemoryDatabaseHelper().Create();

            using (var db = store.OpenSession())
            {
                var importer = Importer.CreateImporter(db, new SeabirdSurveyMapping());
                importer.SkipBadRecords = true;
                importer.Import(@"C:\Work\data\TopCat_BulkDataImport_MLv2.csv");

                var errors = importer.Results
                    .Where(r => !r.Success)
                    .Select(r => r.RecordOutputModel.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                File.WriteAllLines(@"C:\work\data\TopCat_BulkDataImport_MLv2-errors.txt", errors);

                db.SaveChanges();

                imported = db.Query<Record>()
                                .Customize(x => x.WaitForNonStaleResults())
                                .Take(1000).ToList();
            }
        }

        [Test, Explicit] // this isn't seed data, so these tests are (were) only used for the "one-off" import
        public void should_import_expected_number_of_records()
        {
            imported.Count().Should().Be(56);
        }
    }
}
