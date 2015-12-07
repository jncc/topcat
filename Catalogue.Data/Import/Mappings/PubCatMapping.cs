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
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Lucene.Net.Search;
using Newtonsoft.Json;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;

namespace Catalogue.Data.Import.Mappings
{

    public class PubCatMapping : IMapping
    {
        private const string publicationStatusVocab = "http://vocab.jncc.gov.uk/jncc-publication-status";
        private const string publicationCategoryVocab = "http://vocab.jncc.gov.uk/jncc-publication-category";
        private const string reportSeriesNoVocab = "http://vocab.jncc.gov.uk/jncc-report-series-number";
        private const string nhbsVocab = "http://vocab.jncc.gov.uk/NHBS";
        private const string isbnVocab = "http://vocab.jncc.gov.uk/ISBN";
        private const string issnVocab = "http://vocab.jncc.gov.uk/ISSN";

        public IEnumerable<Vocabulary> RequiredVocabularies {
            get
            {
                return new List<Vocabulary>
                {
                    Vocabularies.JnccCategory,
                    Vocabularies.JnccDomain,
                    Vocabularies.MetadataAdmin,
                    new Vocabulary
                        {
                            Id = publicationStatusVocab,
                            Name = "Publication status",
                            Description = "Describes various properties of JNCC Publications",
                            Controlled = true,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>
                                {
                                    new VocabularyKeyword {Value = "Free"},
                                    new VocabularyKeyword {Value = "Discontinued"}
                                }

                        },
                        new Vocabulary
                        {
                            Id = publicationCategoryVocab,
                            Name = "Publication category",
                            Description = "Publication category",
                            Controlled = true,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>()
                        },
                        new Vocabulary
                        {
                            Id = reportSeriesNoVocab,
                            Name = "JNCC Report series number",
                            Description = "JNCC Report series number",
                            Controlled = false,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>()
                        },
                        new Vocabulary
                        {
                            Id = nhbsVocab,
                            Name = "NHBS Numbers",
                            Description = "NHBS Number",
                            Controlled = false,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>()
                        },
                        new Vocabulary
                        {
                            Id = isbnVocab,
                            Name = "ISBN Numbers",
                            Description = "ISBN Numbers",
                            Controlled = false,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>()
                        },
                        new Vocabulary
                        {
                            Id = issnVocab,
                            Name = "ISSN Numbers",
                            Description = "ISSN Numbers",
                            Controlled = false,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>()
                        }
                };
            }

        }

        internal class GeminiMap : CsvClassMap<Metadata>
        {
            public override void CreateMap()
            {
                Map(m => m.Title).Name("Title");
                Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
                {
                    string name = row.GetField("Authors");
                    string email = String.Empty;
                    string role = "author";

                    return new ResponsibleParty { Name = name, Email = email, Role = role };
                });
                Map(m => m.Abstract).ConvertUsing(row =>
                {
                    var sb = new StringBuilder();



                    if (!String.IsNullOrWhiteSpace(row.GetField("ShortSummary")))
                    {              
                        sb.AppendLine(row.GetField("ShortSummary"));
                        sb.AppendLine();
                    }

                    var subtitle = row.GetField("Subtitle");
                    if (!String.IsNullOrWhiteSpace(subtitle))
                    {
                        sb.AppendLine("#### Subtitle");
                        sb.AppendLine(subtitle);
                        sb.AppendLine();
                    }

                    sb.AppendLine(row.GetField("ParsedPageContent"));
                    sb.AppendLine();

                    var citation = row.GetField("Citation");
                    var comment = row.GetField("Comment");

                    if (!string.IsNullOrWhiteSpace(citation))
                    {
                        sb.AppendLine("#### Citation");
                        sb.AppendLine(citation);
                    }

                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        sb.AppendLine("#### Comment");
                        sb.AppendLine(row.GetField("Comment"));
                    }

                    var bibliographicInfo = row.GetField("BibliographicInfo");
                    if (!String.IsNullOrWhiteSpace(bibliographicInfo))
                    {
                        sb.AppendLine("#### Bibliographic Info");
                        sb.AppendLine(bibliographicInfo);
                        
                    }

                    return sb.ToString();
                });
                
                Map(m => m.DatasetReferenceDate)
                    .ConvertUsing(
                        row =>
                            RecordValidator.IsValidDate(row.GetField("PublicationDate"))
                                ? row.GetField("PublicationDate")
                                : String.Empty);

                Map(m => m.Keywords).ConvertUsing(GetKeywords);
                //todo: needed to pass validation
                Map(m => m.ResourceLocator).ConvertUsing(row => "http://jncc.defra.gov.uk/publications-catalogue/");
                Map(m => m.DataFormat).ConvertUsing(row => "Documents");
                Map(m => m.ResourceType).ConvertUsing(row => "publication");
            }


            private List<MetadataKeyword> GetKeywords(ICsvReaderRow row)
            {
                var keywords = new List<MetadataKeyword>();

                keywords.Add(new MetadataKeyword(){Value = "Improve", Vocab = Vocabularies.MetadataAdmin.Id});

                keywords.AddRange(ParsePageKeywords(row.GetField("Keywords")));

                AddKeyword(keywords, nhbsVocab, row.GetField("NhbsNumber"));
                AddKeyword(keywords, isbnVocab, row.GetField("IsbnNumber"));
                AddKeyword(keywords, issnVocab, row.GetField("IssnNumber"));
                AddKeyword(keywords, reportSeriesNoVocab, row.GetField("JnccReportSeriesNumber"));

                if (row.GetField("Free") == "1")
                {
                    AddKeyword(keywords, publicationStatusVocab, "Free");
                }

                if (row.GetField("Discontinued") == "1")
                {
                    AddKeyword(keywords, publicationStatusVocab, "Discontinued");
                }

                keywords.AddRange(
                    GetDomain(keywords.Where(x => x.Vocab == publicationCategoryVocab).ToList()));
                AddKeyword(keywords, Vocabularies.JnccCategory.Id, "JNCC Publications");

                return keywords;
            }

            private void AddKeyword(List<MetadataKeyword> keywords, string vocab, string value)
            {
                if (value.IsBlank()) return;

                keywords.Add(new MetadataKeyword()
                {
                    Vocab = vocab,
                    Value = value
                });
            }


            private List<MetadataKeyword> GetDomain(List<MetadataKeyword> keywords)
            {
                var domainVocab = Vocabularies.JnccDomain.Id;

                var result = new List<MetadataKeyword>();

                if (keywords.Any(k => (new[] { "Species Status"
                    , "Higher plants"
                    , "Lower plants"
                    , "Mammals"
                    , "Grassland"
                    , "Heathland"
                    , "Lowland Wetland"
                    , "Uplands"
                    , "Woodland"
                    , "Geological Conservation Review Series"
                    , "Urban"}).Contains(k.Value, StringComparer.InvariantCultureIgnoreCase)))
                {
                    result.Add(new MetadataKeyword() { Value = "Terrestrial", Vocab = domainVocab});
                }

                if (keywords.Any(k => (new[] { "Aquatic plants", "Freshwater" }).Contains(k.Value, StringComparer.InvariantCultureIgnoreCase)))
                {
                    result.Add(new MetadataKeyword() { Value = "Freshwater", Vocab = domainVocab });
                }

                if (keywords.Any(k => (new[] { "Coastal & Estuarine", "Marine" }).Contains(k.Value, StringComparer.InvariantCultureIgnoreCase)))
                {
                    result.Add(new MetadataKeyword() { Value = "Marine", Vocab = domainVocab });
                }

                if (keywords.Any(k => (new[] { "Pollution" }).Contains(k.Value, StringComparer.InvariantCultureIgnoreCase)
                    && !(new[] { "Pesticides and Toxic Substances" }).Contains(k.Value, StringComparer.InvariantCultureIgnoreCase)))
                {
                    result.Add(new MetadataKeyword() {Value = "Atmosphere", Vocab = domainVocab});
                }


                if (!result.Any()) result.Add(new MetadataKeyword() {Value = "To Do!", Vocab = domainVocab});

                return result;
            }

            private IEnumerable<MetadataKeyword> ParsePageKeywords(string input)
            {

                if (input.IsBlank()) return new List<MetadataKeyword>();

                return (from m in Regex.Matches(input, @"\{(.*?)\}").Cast<Match>()
                        let keyword = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1).First().Split(',')
                        from k in keyword
                        where k.IsNotBlank()
                        select new MetadataKeyword
                        {
                            Vocab = publicationCategoryVocab,
                            Value = k.Replace("&", "and")
                                .ToCharArray()
                                .Where(c => !char.IsPunctuation(c))
                                .Aggregate("", (current, c) => current + c)
                                .Trim()
                        });
            }

        }

        internal class RecordMap : CsvClassMap<Record>
        {
            public override void CreateMap()
            {
                Map(m => m.Path).Name("Path");
                Map(m => m.TopCopy).ConvertUsing(row => true);
                Map(m => m.Status).ConvertUsing(row => Status.Publishable);
                Map(m => m.Notes).ConvertUsing(row =>
                {
                    var notes = new StringBuilder();
                    notes.AppendLine("PageId: " + row.GetField("PageId"));

                    if (!RecordValidator.IsValidDate(row.GetField("PublicationDate")))
                    {
                        notes.AppendLine();
                        notes.AppendLine("Invalid dataset reference date (Publication date) : " +
                                         row.GetField("PublicationDate"));
                    }

                    return notes.ToString();
                });

                References<GeminiMap>(m => m.Gemini);
            }
        }

        public void Apply(CsvConfiguration config)
        {
            config.Delimiter = "\t";
            config.QuoteAllFields = true;
            config.TrimFields = true;
            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<GeminiMap>();
        }
    }


    [Explicit]
    internal class when_importing_pubcat_data
    {

        List<Record> imported;

        [TestFixtureSetUp]
        public void RunPubcatImport()
        {
            var store = new InMemoryDatabaseHelper().Create();

            using (IDocumentSession db = store.OpenSession())
            {
                var importer = Importer.CreateImporter<PubCatMapping>(db);
                importer.SkipBadRecords = true;

                importer.Import(@"C:\Working\pubcat.csv");

                var errors = importer.Results
                   .Where(r => !r.Success)
                   .Select(r => r.Record.Gemini.Title + Environment.NewLine + JsonConvert.SerializeObject(r.Validation) + Environment.NewLine);
                File.WriteAllLines(@"C:\working\pubcat-errors.txt", errors);

                db.SaveChanges();

                imported = db.Query<Record>()
                             .Customize(x => x.WaitForNonStaleResults())
                             .Take(1050).ToList();
            }
        }

        [Test]
        public void should_import_all_records()
        {
            imported.Count().Should().Be(716);
        }
    }

}
