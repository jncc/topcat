using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using Catalogue.Gemini.DataFormats;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using CsvHelper.Configuration;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Catalogue.Data.Import.Mappings
{
    public class ActivitiesMapping : IMapping
    {
        public IEnumerable<Vocabulary> RequiredVocabularies
        {
            get
            {
                return new List<Vocabulary>
                {
                    Vocabularies.JnccCategory,
                    Vocabularies.JnccDomain,
                    new Vocabulary()
                    {
                        Id = "http://www.bmapa.org/documents/BMAPA_Glossary.pdf",
                        Name = "BMAPA Marine Aggregate Terminology",
                        Description = "See http://www.bmapa.org/documents/BMAPA_Glossary.pdf",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },

                    new Vocabulary()
                    {
                        Id = "http://www.fao.org/fi/glossary/aquaculture/en",
                        Name = "FAO Glossary of Aquaculture ",
                        Description = "See http://www.fao.org/faoterm/collection/aquaculture/en/",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },

                    new Vocabulary()
                    {
                        Id = "http://www.generalcable.com/GeneralCable/en-US/Resources/Glossary",
                        Name = "General Cable Glossary",
                        Description = "See http://www.generalcable.com/GeneralCable/en-US/Resources/Glossary/",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },

                    new Vocabulary()
                    {
                        Id = "http://www.snh.org.uk/publications/on-line/heritagemanagement/erosion/7.1.shtml",
                        Name = "SNH Erosion Glossary",
                        Description = "See http://www.snh.org.uk/publications/on-line/heritagemanagement/erosion/7.1.shtml",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary()
                    {
                        Id = "http://evidence.environment-agency.gov.uk/FCERM/Libraries/Fluvial_Documents/Glossary.sflb.ashx",
                        Name = "EA Fluvial Glossary",
                        Description = "See http://evidence.environment-agency.gov.uk/FCERM/Libraries/Fluvial_Documents/Glossary.sflb.ashx",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary()
                    {
                        Id = "http://www.bgs.ac.uk/mineralsUK/glossary.html",
                        Name = "BGS Minerals UK Glossary",
                        Description = "See http://www.bgs.ac.uk/mineralsUK/glossary.html",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary()
                    {
                        Id = "http://en.wikipedia.org/wiki/Glossary_of_nautical_terms",
                        Name = "Wikipedia Glossary of Nautical Terms",
                        Description = "See http://en.wikipedia.org/wiki/Glossary_of_nautical_terms",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary()
                    {
                        Id = "http://www.dtic.mil/doctrine/dod_dictionary",
                        Name = "DOD Dictionary of Military Terms",
                        Description = "See http://www.dtic.mil/doctrine/dod_dictionary/",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary()
                    {
                        Id = "http://en.wikipedia.org/wiki/Glossary_of_fishery_terms",
                        Name = "Wikipedia Glossary of Fishery Terms",
                        Description = "See http://en.wikipedia.org/wiki/Glossary_of_fishery_terms",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary()
                    {
                        Id = "http://www.enchantedlearning.com/wordlist/energy.shtml",
                        Name = "Enchanted Learning Energy Word List",
                        Description = "See http://www.enchantedlearning.com/wordlist/energy.shtml",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary()
                    {
                        Id = "http://vocab.jncc.gov.uk/recreation",
                        Name = "JNCC Recreation Vocabulary",
                        Description = "Used by Marine Human Activities. The lineage of this keyword set is uncertain!",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },
                    new Vocabulary()
                    {
                        Id = "http://vocab.jncc.gov.uk/ukho",
                        Name = "UKHO Vocabulary",
                        Description = "Used by Marine Human Activities. The lineage of this keyword set is uncertain!",
                        Controlled = true,
                        Keywords = new List<VocabularyKeyword>(),
                    },
                };
            }
        }

        public void Apply(CsvConfiguration config)
        {
            // see http://joshclose.github.io/CsvHelper/

            config.Delimiter = "\t";
            config.QuoteAllFields = true;
            config.TrimFields = true;
            config.SkipEmptyRecords = true;
            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<GeminiMap>();
        }

        public sealed class RecordMap : CsvClassMap<Record>
        {
            public RecordMap()
            {
                Map(m => m.Path).Name("JNCC Location");
                Map(m => m.TopCopy).ConvertUsing(row => false); // activities data is not top copy
                Map(m => m.Status).ConvertUsing(row => Status.Internal); // activities data is not publishable
                Map(m => m.Notes).Name("JNCC Notes");
                //Map(m => m.Validation).ConvertUsing(row => Validation.Gemini); //Validate to Gemini standard
                //Map(m => m.Review).ConvertUsing<DateTime?>(row => null);
                Map(m => m.Notes).ConvertUsing(row =>
                {
                    var notes = new StringBuilder();
                    notes.AppendLine(row.GetField("JNCC Notes"));
                    notes.AppendLine();
                    notes.AppendLine(row.GetField("Metadata comments"));

                    return notes.ToString();
                });

                References<GeminiMap>(m => m.Gemini);
            }
        }

        public sealed class GeminiMap : CsvClassMap<Metadata>
        {
            public GeminiMap()
            {
                Map(m => m.Title).Name("Resource Title");
                Map(m => m.Abstract).Name("Resource Abstract");
                Map(m => m.TopicCategory)
                    .ConvertUsing(row => row.GetField("Topic Category")
                        .FirstCharToLower() // correct capitalisation
                        .Replace("structures", "structure"));

                Map(m => m.Keywords).ConvertUsing(row =>
                    {
                        string keywordList = row.GetField("Keyword");
                        var keywords = ParseKeywords(keywordList);

                        string activity = row.GetField("Activity");
                        if (!String.IsNullOrWhiteSpace(activity))
                        {
                            keywords.Add(new MetadataKeyword
                            {
                                Vocab = "http://vocab.jncc.gov.uk/human-activity",
                                Value = activity
                            });
                        }

                        return keywords;
                    });

                Map(m => m.TemporalExtent).ConvertUsing(row =>
                    {
                        var raw = row.GetField("Temporal Extent");

                        if (raw.Contains("/"))
                        {
                            var parsed = Regex.Match(raw, @"(.*)/(.*)") // 'from-date/to-date' or just 'single-date'
                                .Groups.Cast<Group>()
                                .Select(g => g.Value)
                                .ToList(); // should be either a single value, or a the value followed by two (possibly empty) from/to values

                            if (parsed.Count == 3)
                                return new TemporalExtent { Begin = parsed.ElementAt(1), End = parsed.ElementAt(2) };
                        }

                        // let's put the single date in the being
                        return new TemporalExtent { Begin = raw, End = "" };
                    });

                Map(m => m.DatasetReferenceDate).ConvertUsing(row => row.GetField("Dataset reference date"));
                Map(m => m.Lineage);
                Map(m => m.ResourceLocator).Name("Web Address");
                Map(m => m.AdditionalInformationSource).ConvertUsing(row =>
                {
                    var s = new StringBuilder();
                    s.AppendLine(row.GetField("Contact Position (job title/ generic role)"));
                    s.AppendLine();
                    s.AppendLine(row.GetField("Telephone number"));
                    s.AppendLine();
                    s.AppendLine(row.GetField("Address"));
                    return s.ToString();
                });
                Map(m => m.DataFormat).ConvertUsing(row => FixUpDataFormats(row.GetField("Data format")));

                Map(m => m.ResponsibleOrganisation).ConvertUsing(row => new ResponsibleParty
                    {
                        Name = row.GetField("Organisation Name"),
                        Email = row.GetField("Email Address"),
                        Role = row.GetField("Responsible Party Role").FirstCharToLower(),
                    });

                Map(m => m.LimitationsOnPublicAccess).Name("Limitations on public access");
                Map(m => m.UseConstraints).Name("Use constraints");
                Map(m => m.SpatialReferenceSystem).Name("Spatial reference system");
                Map(m => m.MetadataDate).ConvertUsing(row =>
                {
                    string d = ImportUtility.ParseDate(row.GetField("Metadata date"));

                    if (d.IsBlank())
                        return DateTime.Now;
                    else
                    {
                        DateTime dt;
                        bool b = DateTime.TryParse(d, out dt);
                        return dt;
                    }
                });
                Map(m => m.ResourceType).ConvertUsing(row => "dataset"); // only use dataset atm
                Map(m => m.MetadataPointOfContact).ConvertUsing(row =>
                {
                    string name = row.GetField("Metadata point of contact");
                    string email = "April.Eassom@jnccc.gov.uk";
                    string role = "pointOfContact";
                    return new ResponsibleParty { Name = name, Email = email, Role = role };
                });
                
                Map(m => m.BoundingBox).ConvertUsing(row =>
                {
                    
                    String strNorth = row.GetField("North");
                    String strEast = row.GetField("East");
                    String strWest = row.GetField("West");
                    String strSouth = row.GetField("South");

                    /*if a bounding box co ordinate is missing don't attempt to convert*/
                    if (String.IsNullOrEmpty(strNorth) || String.IsNullOrEmpty(strEast) ||
                        String.IsNullOrEmpty(strSouth) || String.IsNullOrEmpty(strWest))
                    {
                        return null;
                    }
                    
                    decimal north = Convert.ToDecimal(strNorth);
                    decimal south = Convert.ToDecimal(strSouth);
                    decimal east = Convert.ToDecimal(strEast);
                    decimal west = Convert.ToDecimal(strWest);

                    return new BoundingBox { North = north, South = south, East = east, West = west };
                });
            }

        }

        static List<MetadataKeyword> ParseKeywords(string input)
        {
            string fixedUp = FixUpKeywordMistake(input);

            var keywords = (from each in fixedUp.Split(',') // keywords are separated by commas
                            where each != ":: Keyword" // bad record
                            select ParseKeywordHelper(each)).ToList();

            // remove keywords with no value (?)
            keywords.RemoveAll(x => String.IsNullOrWhiteSpace(x.Value));

            // add the broad category for activities (not included in the source data)
            keywords.Insert(0, new MetadataKeyword
                {
                   Vocab = "http://vocab.jncc.gov.uk/jncc-domain",
                   Value = "Marine"
                });
            keywords.Insert(1, new MetadataKeyword
            {
                Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                Value = "Human Activities"
            });

            return keywords;
        }

        static MetadataKeyword ParseKeywordHelper(string s)
        {
            var vocabAndValue = (from x in s.Trim().Split(new [] {"::"}, StringSplitOptions.None)
                                 select x.Trim()).ToList(); // vocab::value pairs are separated by two colons

            if (vocabAndValue.Count <= 1) // no vocab (just a value)
            {
                return new MetadataKeyword { Value = vocabAndValue.Single() };
            }
            else
            {
                return new MetadataKeyword
                    {
                        Vocab = MapSourceVocabToRealVocab(vocabAndValue.ElementAt(0)),
                        Value = vocabAndValue.ElementAt(1),
                    };
            }
        }

        public static string MapSourceVocabToRealVocab(string vocab)
        {
            var map = new Dictionary<string, string>
                {
                    { "BMAPA",  "http://www.bmapa.org/documents/BMAPA_Glossary.pdf" },
                    { "FAO",  "http://www.fao.org/fi/glossary/aquaculture/en" },
                    { "General Cable",  "http://www.generalcable.com/GeneralCable/en-US/Resources/Glossary" },
                    { "SNH",  "http://www.snh.org.uk/publications/on-line/heritagemanagement/erosion/7.1.shtml" },
                    { "EA",  "http://evidence.environment-agency.gov.uk/FCERM/Libraries/Fluvial_Documents/Glossary.sflb.ashx" },
                    { "BGS",  "http://www.bgs.ac.uk/mineralsUK/glossary.html" },
                    { "WikiShipList",  "http://en.wikipedia.org/wiki/Glossary_of_nautical_terms" },
                    { "WikiList",  "http://en.wikipedia.org/wiki/Glossary_of_nautical_terms" },
                    { "Wiki List",  "http://en.wikipedia.org/wiki/Glossary_of_nautical_terms" },
                    { "DOD",  "http://www.dtic.mil/doctrine/dod_dictionary" },
                    { "Oil&GasUK",  "http://www.oilandgasuk.co.uk/glossary.cfm" },
                    { "WikiFish",  "http://en.wikipedia.org/wiki/Glossary_of_fishery_terms" },
                    { "Energy",  "http://www.enchantedlearning.com/wordlist/energy.shtml" },
                    { "GeneralList",  "" },
                    { "RecreationList",  "http://vocab.jncc.gov.uk/recreation" },
                    { "UKHOList",  "http://vocab.jncc.gov.uk/ukho" },
                };

            string real = (from p in map
                           let a = p.Key.ToLower()
                           let b = vocab.ToLower()
                           where a == b || a + " list" == b || a + "-list" == b
                           select p.Value).SingleOrDefault();

            if (real == null)
                throw new Exception("Unsupported vocab " + vocab);

            return real;
        }

        static string FixUpDataFormats(string value)
        {
            switch (value)
            {
                case "Geospatial (vector polyline)":
                case "Geospatial (vector point, line and polygon)":
                    return "Geospatial (vector polygon)";
                default:
                    return value;
            }
        }

        static string FixUpKeywordMistake(string input)
        {
            var split = input.Split(',');
            var keywordsInActivitiesEncoding = FixUpKeywordMistakeHelper(split);
            return String.Join(", ", keywordsInActivitiesEncoding);
        }

        static IEnumerable<string> FixUpKeywordMistakeHelper(string[] split)
        {
            string vocab = "";
            string value = "";
            foreach (var s in split)
            {
                if (s.Contains("::"))
                {
                    var pair = s.Split(new[] {"::"}, StringSplitOptions.None);
                    vocab = pair[0];
                    value = pair[1];
                }
                else
                {
                    value = s;
                }

                yield return vocab.Trim() + ":: " + value.Trim();
            }
        }

        [Test]
        public void fix_up_keyword_mistake_test()
        {
            string input1 = "WikiShipList:: Ship, Automatic Identification System";
            string fixed1 = FixUpKeywordMistake(input1);
            fixed1.Should().Be("WikiShipList:: Ship, WikiShipList:: Automatic Identification System");

            string input2 = "BMAPA list:: Maintenance Dredging,  Capital Dredging,  Dredging, Oil&GasUK List:: Oil, Natural Gas, Pipelines, Construction, Coastal Management, Coastal Defence";
            string fixed2 = FixUpKeywordMistake(input2);
            fixed2.Should().Be("BMAPA list:: Maintenance Dredging, BMAPA list:: Capital Dredging, BMAPA list:: Dredging, Oil&GasUK List:: Oil, Oil&GasUK List:: Natural Gas, Oil&GasUK List:: Pipelines, Oil&GasUK List:: Construction, Oil&GasUK List:: Coastal Management, Oil&GasUK List:: Coastal Defence");
        }
    }

    [Explicit] // this isn't seed data, so these tests are (were) only used for the "one-off" import
    class when_importing_activities_data
    {
        List<Record> imported;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var store = new InMemoryDatabaseHelper().Create();

            using (var db = store.OpenSession())
            {
                var importer = Importer.CreateImporter<ActivitiesMapping>(db);
                importer.SkipBadRecords = true; // see log for skipped bad records
                importer.Import(@"C:\work\activities\Catalogue2.txt");

                var errors = importer.Results
                    .Where(r => !r.Success)
                    .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                File.WriteAllLines(@"C:\work\activities\activities-errors.txt", errors);

                db.SaveChanges();

                imported = db.Query<Record>()
                             .Customize(x => x.WaitForNonStaleResults())
                             .Take(1000).ToList();
            }
        }

        [Test]
        public void should_import_every_record()
        {
            imported.Count().Should().Be(108);
        }

        [Test]
        public void should_import_all_records_as_non_top_copy()
        {
            imported.Any(r => r.TopCopy).Should().BeFalse();
        }

        [Test]
        public void should_import_all_records_as_non_publishable()
        {
            imported.Any(r => r.Status == Status.Publishable).Should().BeFalse();
        }

        [Test]
        public void should_import_notes()
        {
            imported.Any(r => r.Notes.Contains("Further information can be found here")).Should().BeTrue();
        }

        [Test]
        public void should_import_gemini_records_for_all_records()
        {
            imported.All(r => r.Gemini != null).Should().BeTrue();
        }

        [Test]
        public void should_import_title()
        {
            imported.Select(r => r.Gemini.Title).Should().Contain("Anonymised AIS derived track lines 2011");
        }

        [Test]
        public void should_import_first_and_last_record()
        {
            imported.First().Gemini.Title.Should().Be("Anonymised AIS derived track lines 2011");
            imported.Last().Gemini.Title.Should().Be("Windfarm Cable routes");
        }

        [Test]
        public void should_import_abstract()
        {
            imported.Select(r => r.Gemini.Abstract)
                .Should().Contain(a => a != null && a.Contains("This dataset displays the location of marinas"));
        }

        [Test]
        public void should_import_topic_category()
        {
            imported.Count(r => r.Gemini.TopicCategory == "utilitiesCommunication").Should().BeGreaterThan(3);
            imported.Count(r => r.Gemini.TopicCategory == "transportation").Should().BeGreaterThan(3);
            imported.Count(r => r.Gemini.TopicCategory == "structure").Should().BeGreaterThan(3);
        }

        [Test]
        public void should_import_jncc_category_keyword()
        {
            imported.SelectMany(r => r.Gemini.Keywords).Should().Contain(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-category" && k.Value == "Human Activities");
        }

        [Test]
        public void should_import_keywords_that_have_no_vocab_namespace()
        {
            imported.SelectMany(r => r.Gemini.Keywords)
                .Should().Contain(k => k.Vocab.IsBlank() && k.Value == "Extraction");
        }

        [Test]
        public void should_import_keywords_with_either_an_http_namespace_or_no_namespace()
        {
            // a test to check that we're converting the short vocab list names to a suitable http namespace 
            imported.SelectMany(r => r.Gemini.Keywords)
                .All(k => k.Vocab.IsBlank() || k.Vocab.StartsWith("http://"))
                .Should().BeTrue();
        }

        [Test]
        public void should_import_keywords_with_wikipedia_glossary_of_nautical_terms_namespace()
        {
            imported.SelectMany(r => r.Gemini.Keywords)
                .Should().Contain(k => k.Vocab == "http://en.wikipedia.org/wiki/Glossary_of_nautical_terms");
        }

        [Test]
        public void should_import_temporal_extent()
        {
            imported.Should().Contain(r =>
                r.Gemini.TemporalExtent.Begin.StartsWith("2006") && r.Gemini.TemporalExtent.End.StartsWith("2010"));
        }

        [Test]
        public void should_not_import_temporal_extent_with_multiple_dates()
        {
            // todo remove https://github.com/JNCC-dev-team/catalogue/issues/18
           // imported.Should().NotContain(r => r.Gemini.TemporalExtent.Begin == "Jan, Mar, Jun, Sep 2010");
        }

        [Test]
        public void should_import_dataset_reference_date()
        {
            imported.Select(r => r.Gemini.DatasetReferenceDate).Should().Contain("2015-01-07");
        }

        [Test]
        public void should_import_lineage()
        {
            imported.Should().Contain(r => r.Gemini.Lineage == "Anchorage areas provided by Chamber of Shipping.");
        }

        [Test]
        public void should_import_data_format()
        {
            imported.Should().Contain(r => r.Gemini.DataFormat == "Geospatial (vector polygon)");
            imported.Should().Contain(r => r.Gemini.DataFormat == "Geospatial (vector line)");
            imported.Should().Contain(r => r.Gemini.DataFormat == "Microsoft Excel for Windows");
        }

        [Test]
        public void should_import_only_known_data_formats()
        {
            imported.Select(r => r.Gemini.DataFormat)
                .Should().OnlyContain(x => DataFormats.Known.SelectMany(g => g.Formats).Any(f => f.Name == x));
        }

        [Test]
        public void should_import_responsible_organisation()
        {
            imported.Select(r => r.Gemini.ResponsibleOrganisation)
                .Should().Contain(o => o.Name == "The Crown Estate"
                    && o.Email.EndsWith("@thecrownestate.co.uk") && o.Role.Equals("owner", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void should_import_limitations_on_public_access()
        {
            imported.Select(r => r.Gemini.LimitationsOnPublicAccess)
                .Should().Contain("Data publically available on the Crown Estate website");
        }

        [Test]
        public void should_import_use_contraints()
        {
            imported.Select(r => r.Gemini.UseConstraints)
                .Should().Contain(s => s.StartsWith("By using the data you agree to the following terms & conditions"));

            imported.Select(r => r.Gemini.UseConstraints)
                .Should().Contain("no conditions apply");

            imported.Select(r => r.Gemini.UseConstraints).Any(s => s.IsBlank())
                .Should().BeFalse();
        }

        //[Test]
        public void should_import_spatial_reference_system()
        {
            // not done yet
            imported.Select(r => r.Gemini.SpatialReferenceSystem)
                .Should().Contain("todo");
        }

        [Test]
        public void should_import_metadata_date_correctly()
        {
            // make sure it parsed the right way round
            imported.Select(r => r.Gemini.MetadataDate).Should().Contain(dt => dt.Year == 2015);
        }

        [Test]
        public void source_identifiers_should_be_empty()
        {
            // activities records have no IDs other than the ones we generate
            imported.All(r => r.SourceIdentifier == null).Should().BeTrue();
        }

        [Test]
        public void all_records_should_have_a_valid_path()
        {
//            Uri uri; // need this for Uri.TryCreate; not actually using it
//
//            imported.Count(r => Uri.TryCreate(r.Path, UriKind.Absolute, out uri))
//                    .Should().Be(189);
        }
    }
}
