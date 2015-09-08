using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Export;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using Catalogue.Gemini.Model;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Catalogue.Data.Import.Mappings
{
    public class MeowMapping : IMapping
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
            config.PrefixReferenceHeaders = true;
//            TypeConverterFactory.AddConverter<List<MetadataKeyword>>(new Exporter.MetadataKeywordConverter());
//            TypeConverterFactory.AddConverter<List<Extent>>(new Exporter.ExtentListConverter());

            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<GeminiMap>();

            // there's no id field because they deleted it
            config.WillThrowOnMissingField = false;
        }

        public sealed class GeminiMap : CsvClassMap<Metadata>
        {
            public GeminiMap()
            {
                Map(m => m.Title).Name("Gemini.Abstract");
                Map(m => m.Abstract).ConvertUsing(row => "This dataset was created as part of the Making Earth Observation Work project. The data and metadata record was provided by Environment Systems Ltd.");
                Map(m => m.TopicCategory).ConvertUsing(row =>
                {
                    string value = row.GetField("Gemini.TopicCategory");
                    return value.Replace("inland waters", "inlandWaters").Trim();
                });
                Map(m => m.Keywords).ConvertUsing(row => new List<MetadataKeyword>
                    {
                        new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Terrestrial" },
                        new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Earth Observation"},
                    });
                Map(m => m.TemporalExtent).ConvertUsing(row => new TemporalExtent
                    {
                        Begin = row.GetField("TemporalExtent.Begin"),
                        End = row.GetField("TemporalExtent.End")
                    });
                Map(m => m.DatasetReferenceDate).ConvertUsing(row => "2015-05-14");
                Map(m => m.Lineage).Name("Gemini.Lineage");
                Map(m => m.SpatialReferenceSystem).ConvertUsing(row =>
                {
                    string value = row.GetField("Gemini.SpatialReferenceSystem");
                    if (value == "OSGB")
                        return "http://www.opengis.net/def/crs/EPSG/0/27700";
                    else
                        return null;
                });
                Map(m => m.DataFormat).Name("Gemini.DataFormat");
                Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
                {
                    string name = row.GetField("ResponsibleOrganisation.Name");
                    string email = row.GetField("ResponsibleOrganisation.Email");
                    string role = row.GetField("ResponsibleOrganisation.Role");

                    return new ResponsibleParty {Name = name, Email = email, Role = role};
                });
                Map(m => m.LimitationsOnPublicAccess).Name("Gemini.LimitationsOnPublicAccess");
                Map(m => m.UseConstraints).Name("Gemini.UseConstraints");
                Map(m => m.MetadataDate).ConvertUsing(row => new DateTime(2015, 6, 1));
                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
                {
                    string name = row.GetField("MetadataPointOfContact.Name");
                    string email = row.GetField("MetadataPointOfContact.Email");
                    string role = "pointOfContact";

                    return new ResponsibleParty { Name = name, Email = email, Role = role };
                });
                Map(m => m.ResourceType).ConvertUsing(row => "dataset");
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
                Map(m => m.TopCopy).ConvertUsing(row => true);
                Map(m => m.Status).ConvertUsing(row => Status.Internal);
                Map(m => m.Security);

                References<GeminiMap>(m => m.Gemini);
            }
        }
    }

    [Explicit] // this isn't seed data, so these tests are (were) only used for the "one-off" import
    class when_importing_meow_spreadsheet
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
                    var importer = Importer.CreateImporter<MeowMapping>(db);
                    importer.SkipBadRecords = true; // see log for skipped bad records
                    importer.Import(@"C:\Work\meow-export-FINAL v3.csv");

                    var errors = importer.Results
                        .Where(r => !r.Success)
                        .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                    File.WriteAllLines(@"C:\work\meow-errors.txt", errors);

                    db.SaveChanges();

                    imported = db.Query<Record>()
                                 .Customize(x => x.WaitForNonStaleResults())
                                 .Take(1000).ToList();

                }
                catch (CsvHelperException ex)
                {
                    string s = (string) ex.Data["CsvHelper"];
                    throw;
                }
            }
        }

        [Test]
        public void should_import_expected_number_of_records()
        {
            imported.Count().Should().Be(700);
        }
    }

}
