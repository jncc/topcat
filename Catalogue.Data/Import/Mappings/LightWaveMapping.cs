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
using CsvHelper.Configuration;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Catalogue.Data.Import.Mappings
{
    /// <summary>
    /// Steps: unknown. Get CSV from someone.
    /// </summary>
    public class LightWaveMapping : IMapping
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
                Map(m => m.TopicCategory).Field("Gemini.TopicCategory", value => value.FirstCharToLower());
                Map(m => m.Keywords).ConvertUsing(row =>
                {
                    string keyword2 = row.GetField("Keyword2");
                    string keyword3 = row.GetField("Keyword3");
                    string keyword4 = row.GetField("Keyword4");
                    string refManCode = row.GetField("Keyword.JNCC Reference Manager Code");

                    var keywords = new List<MetadataKeyword>
                    {
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine"},
                        new MetadataKeyword {Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Light Wave"},
                    };

                    if (keyword2.IsNotBlank())
                        keywords.Add(new MetadataKeyword { Vocab = null, Value = keyword2 });
                    if (keyword3.IsNotBlank())
                        keywords.Add(new MetadataKeyword { Vocab = null, Value = keyword3 });
                    if (keyword4.IsNotBlank())
                        keywords.Add(new MetadataKeyword { Vocab = null, Value = keyword4 });
                    if (refManCode.IsNotBlank())
                        keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/reference-manager-code", Value = refManCode });

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
                    string role = row.GetField("ResponsibleOrganisation.Role").Replace("point of Contact", "pointOfContact").FirstCharToLower().Trim();

                    return new ResponsibleParty { Name = name == "JNCC" ? "Joint Nature Conservation Committee (JNCC)" : name, Email = email, Role = role };
                });
                Map(m => m.LimitationsOnPublicAccess).Field("Gemini.LimitationsOnPublicAccess");
                Map(m => m.UseConstraints).Field("Gemini.UseConstraints");
                Map(m => m.Copyright).Field("Copyright");
                //              Map(m => m.SpatialReferenceSystem).Field("Gemini.SpatialReferenceSystem", value => value == "N/A" ? null : value);
                Map(m => m.Extent).Ignore();
                Map(m => m.MetadataDate).Value(DateTime.Now);
                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
                {
                    //                  string name = "Joint Nature Conservation Committee (JNCC)";
                    //                  string email = "data@jncc.gov.uk";
                    //                  string role = "pointOfContact";
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
                Map(m => m.Path).ConvertUsing(row =>
                {
                    // let's just use the folder of the first resource
                    string path1 = row.GetField("Path1");
                    return Path.GetDirectoryName(path1);
                });
                Map(m => m.TopCopy);
                Map(m => m.Validation).Value(Validation.Gemini);
                Map(m => m.Status).Ignore();
                Map(m => m.Security).Ignore();
                Map(m => m.Review).Ignore();
                Map(m => m.Notes);
                Map(m => m.SourceIdentifier);
                Map(m => m.ReadOnly);

                References<GeminiMap>(m => m.Gemini);
                //References<PublicationInfoMap>(m => m.Gemini);
            }
        }

        public sealed class PublicationInfoMap : CsvClassMap<PublicationInfo>
        {
            public PublicationInfoMap()
            {
                References<OpenDataPublicationInfoMap>(m => m.OpenData);
            }
        }

        public sealed class OpenDataPublicationInfoMap : CsvClassMap<OpenDataPublicationInfo>
        {
            public OpenDataPublicationInfoMap()
            {
                var cols = Enumerable.Range(1, 13).Select(n => "Path" + n).ToList();
                Map(m => m.Resources).ConvertUsing(row =>
                {
                    return cols.Select(col => row.GetField<string>(col)).Where(StringUtility.IsNotBlank).ToList();
                });
            }
        }
    }

    [Explicit]
    class when_importing_light_waw_import
    {
        List<Record> imported;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var store = new InMemoryDatabaseHelper().Create();

            using (var db = store.OpenSession())
            {
                var importer = Importer.CreateImporter(db, new LightWaveMapping());
                importer.SkipBadRecords = true;
                importer.Import(@"C:\Work\data\LightWave_RiskAssessv2.csv");

                var errors = importer.Results
                    .Where(r => !r.Success)
                    .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                File.WriteAllLines(@"C:\work\data\LightWave_RiskAssessv2.csv-errors.txt", errors);

                db.SaveChanges();

                imported = db.Query<Record>()
                                .Customize(x => x.WaitForNonStaleResults())
                                .Take(1000).ToList();
            }
        }

        [Test, Explicit] // this isn't seed data, so these tests are (were) only used for the "one-off" import
        public void should_import_expected_number_of_records()
        {
            imported.Count().Should().Be(10);
        }
    }
}
