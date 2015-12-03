using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Steps: Get Excel sheet, delete "empty" rows from bottom, save as MS-DOS CSV.
    /// </summary>
    public class EuropeanReportingMapping : IMapping
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
                Map(m => m.TopicCategory).Value("environment"); // .Field("Gemini.TopicCategory"); ### TEMP
                Map(m => m.Keywords).ConvertUsing(row =>
                {
                    var keywords = new List<MetadataKeyword>
                    {
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "European Reporting"},
                    };

                    var domains = row.GetField("Gemini.Keywords.Domain").Split(',')
                        .Select(s => s.Trim())
                        .Select(s => new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = s});

//                    string keyword1 = row.GetField("Gemini.Keywords.Keyword1");

//                    if (keyword1.IsNotBlank())
//                        keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/BLAH", Value = keyword1 });

                    return domains.Concat(keywords).ToList();
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
                Map(m => m.DataFormat).Field("Gemini.DataFormat", val => "Comma Separated Values"); // ###TEMP
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
                    string name = row.GetField("MetadataPointOfContact.Name").Trim();
                    string email = row.GetField("MetadataPointOfContact.Email").Trim();
                    string role = row.GetField("MetadataPointOfContact.Role").FirstCharToLower().Trim();

                    return new ResponsibleParty { Name = name, Email = email, Role = role };
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
                Map(m => m.Status).Ignore();
                Map(m => m.Security).Ignore();
                Map(m => m.Review).Ignore();
                Map(m => m.Notes).Ignore();
                Map(m => m.SourceIdentifier).Ignore();
                Map(m => m.ReadOnly).Value(true);

                References<GeminiMap>(m => m.Gemini);
            }
        }
    }

    [Explicit]
    class when_importing_european_reporting_import
    {
        List<Record> imported;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var store = new InMemoryDatabaseHelper().Create();

            using (var db = store.OpenSession())
            {
                var importer = Importer.CreateImporter<EuropeanReportingMapping>(db);
                importer.SkipBadRecords = true;
                importer.Import(@"C:\Work\data\europeanreporting\Article 17 JNCC publishable metadata_riskassessment.csv");

                var errors = importer.Results
                    .Where(r => !r.Success)
                    .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                File.WriteAllLines(@"C:\work\data\europeanreporting\errors.txt", errors);

                db.SaveChanges();

                imported = db.Query<Record>()
                                .Customize(x => x.WaitForNonStaleResults())
                                .Take(1000).ToList();
            }
        }

        [Test, Explicit] // this isn't seed data, so these tests are (were) only used for the "one-off" import
        public void should_import_expected_number_of_records()
        {
            imported.Count().Should().Be(16);
        }
    }
}
