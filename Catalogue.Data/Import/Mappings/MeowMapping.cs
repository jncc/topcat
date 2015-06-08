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
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Catalogue.Data.Import.Mappings
{
    /// <summary>
    ///  Basically the same as the TopcatMapping.
    /// </summary>
    public class MeowMapping  :IMapping
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
            TypeConverterFactory.AddConverter<List<MetadataKeyword>>(new Exporter.MetadataKeywordConverter());
            TypeConverterFactory.AddConverter<List<Extent>>(new Exporter.ExtentListConverter());

            // there's no id field because they deleted it
            config.WillThrowOnMissingField = false;
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
                var importer = Importer.CreateImporter<MeowMapping>(db);
                importer.SkipBadRecords = true; // see log for skipped bad records
                importer.Import(@"C:\Work\meow.csv");

                var errors = importer.Results
                    .Where(r => !r.Success)
                    .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                File.WriteAllLines(@"C:\work\meow-errors.txt", errors);

                db.SaveChanges();

                imported = db.Query<Record>()
                             .Customize(x => x.WaitForNonStaleResults())
                             .Take(1000).ToList();
            }
        }

        [Test]
        public void should_import_expected_number_of_records()
        {
            imported.Count().Should().Be(699);
        }
    }

}
