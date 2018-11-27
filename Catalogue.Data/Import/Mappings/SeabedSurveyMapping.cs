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
    public class SeabedSurveyMapping : IReaderMapping
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

        public void Apply(IReaderConfiguration config)
        {
            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<GeminiMap>();

            config.MissingFieldFound = null;
            config.TrimOptions = TrimOptions.Trim;
        }

        public sealed class GeminiMap : ClassMap<Metadata>
        {
            public GeminiMap()
            {
                Map(m => m.Title).Name("Gemini.Title");
                Map(m => m.Abstract).Name("Gemini.Abstract");
                Map(m => m.TopicCategory).Name("Gemini.TopicCategory").ConvertUsing(row => row.GetField("Gemini.TopicCategory").FirstCharToLower());
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
                            .Select(s => new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/location", Value = s.Trim() });
                        keywords.AddRange(sites);
                    }

                    if (technique.IsNotBlank())
                    {
                        var techniques = site.Split(';')
                            .Select(s => new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/seabed-survey-technique", Value = s.Trim() });
                        keywords.AddRange(techniques);
                    }

                    if (dataType.IsNotBlank())
                        keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/seabed-survey-data-type", Value = dataType });

                    return keywords;
                });
                Map(m => m.TemporalExtent).ConvertUsing(row => new TemporalExtent
                    {
                        Begin = row.GetField("TemporalExtent.Begin"),
                        End = row.GetField("TemporalExtent.End")
                    });
                Map(m => m.DatasetReferenceDate).Constant("2015-09-01");
                Map(m => m.Lineage).Constant("This dataset is an output of a collaborative offshore seabed survey undertaken with Joint Nature Conservation Committee (JNCC).");
                Map(m => m.ResourceLocator).Ignore();
                Map(m => m.AdditionalInformationSource).Ignore();
                Map(m => m.DataFormat).Name("Gemini.DataFormat");
                Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
                    {
                        string name = row.GetField("ResponsibleOrganisation.Name").Trim();
                        string email = row.GetField("ResponsibleOrganisation.Email").Trim();
                        string role = row.GetField("ResponsibleOrganisation.Role").FirstCharToLower().Trim();

                        return new ResponsibleParty { Name = name == "JNCC" ? "Joint Nature Conservation Committee (JNCC)" : name, Email = email, Role = role };
                    });
                Map(m => m.LimitationsOnPublicAccess).Name("Gemini.LimitationsOnPublicAccess");
                Map(m => m.UseConstraints).Name("Gemini.UseConstraints");
                Map(m => m.Copyright).Name("Copyright");
                Map(m => m.SpatialReferenceSystem).Name("Gemini.SpatialReferenceSystem").ConvertUsing(row => row.GetField("Gemini.SpatialReferenceSystem")  == "N/A" ? null : row.GetField("Gemini.SpatialReferenceSystem"));
                Map(m => m.Extent).Ignore();
                Map(m => m.MetadataDate).Constant(DateTime.Now);
                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
                    {
                        string name = "Joint Nature Conservation Committee";
                        string email = "data@jncc.gov.uk";
                        string role = "pointOfContact";
                        return new ResponsibleParty { Name = name, Email = email, Role = role };
                    });
                Map(m => m.ResourceType).Name("Gemini.ResourceType").ConvertUsing(row => row.GetField("Gemini.ResourceType").FirstCharToLower());
                Map(m => m.ResourceType).ConvertUsing(row =>
                    {
                        // resource type isn't provided in this sheet, so work it out - it'll be dataset if there's a bbox
                        string bbn = row.GetField("BoundingBox.North");

                        if (IsBlankBoundingBoxValue(bbn))
                            return "nonGeographicDataset";
                        else
                            return "dataset";
                    });
                Map(m => m.BoundingBox).ConvertUsing(row =>
                    {
                        string n = row.GetField("BoundingBox.North");
                        if (IsBlankBoundingBoxValue(n))
                        {
                            return null;
                        }
                        else
                        {
                            decimal north = Convert.ToDecimal(row.GetField("BoundingBox.North"));
                            decimal south = Convert.ToDecimal(row.GetField("BoundingBox.South"));
                            decimal east = Convert.ToDecimal(row.GetField("BoundingBox.East"));
                            decimal west = Convert.ToDecimal(row.GetField("BoundingBox.West"));

                            return new BoundingBox { North = north, South = south, East = east, West = west };
                        }
                    });
            }
        }

        static bool IsBlankBoundingBoxValue(string b)
        {
            return b.IsBlank() || b.ToLower().Trim() == "n/a";
        }

        public sealed class RecordMap : ClassMap<Record>
        {
            public RecordMap()
            {
                Map(m => m.Id).ConvertUsing(row =>
                {
                    var id = row.GetField("Id");
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        return new Guid().ToString();
                    }
                    return new Guid(id.Trim()).ToString();
                });
                Map(m => m.Path);
                Map(m => m.TopCopy).Constant(true);
                Map(m => m.Validation).Constant(Validation.Gemini);
                Map(m => m.Status).Ignore();
                Map(m => m.Security).Constant(Security.Official);
                Map(m => m.Review);
                Map(m => m.Notes);
                Map(m => m.SourceIdentifier);
                Map(m => m.ReadOnly).Constant(false);

                References<GeminiMap>(m => m.Gemini);
            }
        }

    }

    [Explicit]
    class when_importing_seabed_survey_spreadsheet
    {
        List<Record> imported;

        [OneTimeSetUp]
        public void SetUp()
        {
            var store = new InMemoryDatabaseHelper().Create();

            using (var db = store.OpenSession())
            {
                try
                {
                    var importer = Importer.CreateImporter(db, new SeabedSurveyMapping());
                    importer.SkipBadRecords = true; // see log for skipped bad records
                    importer.Import(@"D:\workspace\test_import.csv");

                    var errors = importer.Results
                        .Where(r => !r.Success)
                        .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                    File.WriteAllLines(@"D:\workspace\test_import.csv.errors.txt", errors);

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
            imported.Count().Should().Be(147);
        }

        [Test, Explicit]
        public void should_import_copyright()
        {
            imported.Should().Contain(r => r.Gemini.Copyright.Contains("© 2014  Joint Nature Conservation Committee"));
        }

        [Test, Explicit]
        public void MakeAnXmlFile()
        {
            var record = imported.Single(r => r.Gemini.Title == @"Cruise report from Braemar Pockmarks (CEND 19x/12)");
            record.Gemini.ResourceLocator = String.Format("http://example.com/{0}", record.Id);
            var xml = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini);
            //var ceh = new global::Catalogue.Gemini.Validation.Validator().Validate(xml);
            string filename = "topcat-record-" + record.Id.ToString().ToLower() + ".xml";
            xml.Save(Path.Combine(@"D:\workspace", filename));
        }
    }

}
