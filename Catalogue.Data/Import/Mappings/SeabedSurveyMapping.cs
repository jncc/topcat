using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Steps: 
    /// Get the spreadsheet from Mike
    /// Save as CSV (MS-DOS)
    /// </summary>
    public class SeabedSurveyMapping : IMapping
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
                        Id = "http://vocab.jncc.gov.uk/seabed-survey-cruise-id",
                        Name = "Cruise ID",
                        Description = "Management keyword for Offshore Seabed Survey",
                        Controlled = false,
                        PublicationDate = "2015",
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary
                    {
                        Id = "http://vocab.jncc.gov.uk/seabed-survey-vessel",
                        Name = "Survey Vessel",
                        Description = "Management keyword for Offshore Seabed Survey",
                        Controlled = false,
                        PublicationDate = "2015",
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary
                    {
                        Id = "http://vocab.jncc.gov.uk/location",
                        Name = "Location",
                        Description = "Location keyword. Could be moved to the Gemini location field.",
                        Controlled = false,
                        PublicationDate = "2015",
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary
                    {
                        Id = "http://vocab.jncc.gov.uk/seabed-survey-technique",
                        Name = "Seabed Survey Technique",
                        Description = "Used by MESH",
                        Keywords = new List<VocabularyKeyword>()
                    },
                    new Vocabulary
                    {
                        Id = "http://vocab.jncc.gov.uk/seabed-survey-data-type",
                        Name = "Seabed Survey Data Type",
                        Description = "Management keyword for Offshore Seabed Survey",
                        Publishable = false,
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "Raw" },
                            new VocabularyKeyword { Value = "Processed" }
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
                    string surveyVessel = row.GetField("Gemini.Keywords.Survey vessel ");
                    string cruiseId = row.GetField("Gemini.Keywords.Cruise ID ");
                    string site = row.GetField("Gemini.Keywords.Site ");
                    string technique = row.GetField("Gemini.Keywords.Seabed Survey Technique  ");
                    string dataType = row.GetField("Gemini.Keywords.Data Type  ");

                    var keywords = new List<MetadataKeyword>
                    {
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine"},
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Offshore Seabed Survey"},
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/seabed-survey-vessel", Value = surveyVessel},
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/seabed-survey-cruise-id", Value = cruiseId },
                    };

                    if (site.IsNotBlank())
                    {
                        var sites = site.Split(';')
                            .Select(s => s.Trim())
                            .Select(s => new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/location", Value = s });
                        keywords.AddRange(sites);
                    }

                    if (technique.IsNotBlank())
                        keywords.Add(new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/seabed-survey-technique", Value = technique });

                    if (dataType.IsNotBlank())
                        keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/seabed-survey-data-type", Value = dataType });

                    return keywords;
                });
                Map(m => m.TemporalExtent).ConvertUsing(row => new TemporalExtent
                    {
                        Begin = row.GetField("TemporalExtent.Begin"),
                        End = row.GetField("TemporalExtent.End")
                    });
                Map(m => m.DatasetReferenceDate).Value("2015-09-01");
                Map(m => m.Lineage).Value("This dataset is an output of a collaborative offshore seabed survey undertaken by Joint Nature Conservation Committee (JNCC).");
                Map(m => m.ResourceLocator).Ignore();
                Map(m => m.AdditionalInformationSource).Ignore();
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
                        string name = "Joint Nature Conservation Committee";
                        string email = "data@jncc.gov.uk";
                        string role = "pointOfContact";
                        return new ResponsibleParty { Name = name, Email = email, Role = role };
                    });
                Map(m => m.ResourceType).Field("Gemini.ResourceType", value => value.FirstCharToLower());
                Map(m => m.BoundingBox).ConvertUsing(row =>
                    {
                        string n = row.GetField("BoundingBox.North");
                        if (n.IsNotBlank() && n != "NA")
                        {
                            decimal north = Convert.ToDecimal(row.GetField("BoundingBox.North"));
                            decimal south = Convert.ToDecimal(row.GetField("BoundingBox.South"));
                            decimal east = Convert.ToDecimal(row.GetField("BoundingBox.East"));
                            decimal west = Convert.ToDecimal(row.GetField("BoundingBox.West"));

                            return new BoundingBox { North = north, South = south, East = east, West = west };
                        }
                        else
                        {
                            return null;
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
                Map(m => m.Status);
                Map(m => m.Security);
                Map(m => m.Review);
                Map(m => m.Notes);
                Map(m => m.SourceIdentifier);
                Map(m => m.ReadOnly);

                References<GeminiMap>(m => m.Gemini);
            }
        }

    }

    class when_importing_seabed_survey_spreadsheet
    {
        List<Record> imported;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var store = new InMemoryDatabaseHelper().Create();

            using (var db = store.OpenSession())
            {
                try
                {
                    var importer = Importer.CreateImporter<SeabedSurveyMapping>(db);
                    importer.SkipBadRecords = true; // see log for skipped bad records
                    importer.Import(@"C:\Work\data\Offshore_survey_TopCat_data_part1.csv");

                    var errors = importer.Results
                        .Where(r => !r.Success)
                        .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                    File.WriteAllLines(@"C:\work\data\offshore-seabed-survey-errors.txt", errors);

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
            imported.Count().Should().Be(276);
        }


        [Test, Explicit]
        void MakeAnXmlFile()
        {
            var record = imported.Single(r => r.Gemini.Title == "Locations of grab samples with Particle Size Analysis (PSA) results from Bassurelle Sandbank SCI");
            record.Gemini.ResourceLocator = String.Format("http://example.com/{0}", record.Id);
            var xml = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini);
            //var ceh = new global::Catalogue.Gemini.Validation.Validator().Validate(xml);
            string filename = "topcat-record-" + record.Id.ToString().ToLower() + ".xml";
            xml.Save(Path.Combine(@"C:\work", filename));
        }
    }

}
