﻿using System;
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
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/location", Value = site },
                    };

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
                Map(m => m.Lineage).Field("Gemini.Lineage");
                Map(m => m.ResourceLocator).Ignore();
                Map(m => m.AdditionalInformationSource).Ignore();
                Map(m => m.DataFormat).Field("Gemini.DataFormat");
                Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
                    {
                        string name = row.GetField("ResponsibleOrganisation.Name");
                        string email = row.GetField("ResponsibleOrganisation.Email");
                        string role = row.GetField("ResponsibleOrganisation.Role").FirstCharToLower();

                        return new ResponsibleParty { Name = name, Email = email, Role = role };
                    });
                Map(m => m.LimitationsOnPublicAccess).Field("Gemini.LimitationsOnPublicAccess");
                Map(m => m.UseConstraints).Field("Gemini.UseConstraints");
                Map(m => m.SpatialReferenceSystem).Field("Gemini.SpatialReferenceSystem", value => value == "N/A" ? null : value);
                Map(m => m.Extent).Ignore();
                Map(m => m.MetadataDate).Value(DateTime.Now);
                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
                    {
                        string name = row.GetField("JNCC");
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

    [Explicit] // this isn't seed data, so these tests are (were) only used for the "one-off" import
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
                    importer.Import(@"C:\work\Offshore_survey_TopCat_data_MN.csv");

                    var errors = importer.Results
                        .Where(r => !r.Success)
                        .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                    File.WriteAllLines(@"C:\work\seabed-survey-errors.txt", errors);

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

        [Test]
        public void should_import_expected_number_of_records()
        {
            imported.Count().Should().Be(45);
        }
    }

}