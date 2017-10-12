using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using NUnit.Framework.Constraints;

namespace Catalogue.Data.Import.Mappings
{
    /// <summary>
    /// Steps: Get Excel sheet from Dave. Open in Open Office, save as Unicode CSV.
    /// </summary>
    public class DaveChambersMapping : IMapping
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
                    string domain = row.GetField("Gemini.Keywords.Domain");
                    // split and correct comma-separated domains
                    var domains = domain.Split(',').Select(s => s.Trim().Replace("Terrrestrial", "Terrestrial"));

                    string category = row.GetField("Gemini.Keywords.JNCCCategory");
                    string others = row.GetField("Gemini.Keywords.Category");

                    var keywords = new List<MetadataKeyword>();

                    foreach (var d in domains)
                    {
                        keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = d });
                    }

                    keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = category });

                    if (others.IsNotBlank())
                    {
                        foreach (var keyword in others.Split(',').Where(s => s.IsNotBlank()).Select(s => s.Trim()))
                        {
                            keywords.Add(new MetadataKeyword { Vocab = "", Value = keyword });
                        }
                    }

                    return keywords;
                });
                Map(m => m.TemporalExtent).ConvertUsing(row => new TemporalExtent
                {
                    Begin = FixUpUkStyleDate(row.GetField("TemporalExtent.Begin")),
                    End = FixUpUkStyleDate(row.GetField("TemporalExtent.End"))
                });
                Map(m => m.DatasetReferenceDate).Field("Gemini.DatasetReferenceDate", FixUpUkStyleDate);
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
                Map(m => m.Copyright).Field("Copyright");
                Map(m => m.SpatialReferenceSystem).Field("Gemini.SpatialReferenceSystem", value => value == "N/A" ? null : value);
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
                Map(m => m.Path);
                Map(m => m.TopCopy);
                Map(m => m.Validation).Value(Validation.Gemini);
                Map(m => m.Status).Ignore();
                Map(m => m.Security).Ignore();
                Map(m => m.Review).Ignore();
                Map(m => m.Notes);
                Map(m => m.SourceIdentifier);
                Map(m => m.ReadOnly);

                References<GeminiMap>(m => m.Gemini);
            }
        }

        public static string FixUpUkStyleDate(string input)
        {
            var match = Regex.Match(input, @"(\d\d)/(\d\d)/(\d\d\d\d)");

            if (match.Success)
            {
                return match.Groups[3].Value + "-" + match.Groups[2].Value + "-" + match.Groups[1].Value;
            }
            else
            {
                return input;
            }
        }
    }

    [Explicit]
    class when_importing_dave_chambers_import
    {
        List<Record> imported;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var store = new InMemoryDatabaseHelper().Create();

            using (var db = store.OpenSession())
            {
                var importer = Importer.CreateImporter(db, new DaveChambersMapping());
                importer.SkipBadRecords = true;
                importer.Import(@"C:\Work\data\Data Services metadata for top cat entry.csv");

                var errors = importer.Results
                    .Where(r => !r.Success)
                    .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                File.WriteAllLines(@"C:\work\data\dave-chambers-import-errors.txt", errors);

                db.SaveChanges();

                imported = db.Query<Record>()
                                .Customize(x => x.WaitForNonStaleResults())
                                .Take(1000).ToList();
            }
        }

        [Test, Explicit] // this isn't seed data, so these tests are (were) only used for the "one-off" import
        public void should_import_expected_number_of_records()
        {
            imported.Count().Should().Be(54);
        }

        [Test, Explicit]
        public void should_convert_uk_style_dates_ok()
        {
            string ukStyleDate = "01/05/2016";

            DaveChambersMapping.FixUpUkStyleDate(ukStyleDate).Should().Be("2016-50-01");
        }


    }
}
