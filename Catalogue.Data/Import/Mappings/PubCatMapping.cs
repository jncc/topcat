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
using Newtonsoft.Json;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;

namespace Catalogue.Data.Import.Mappings
{
    public class PubCatMapping : IMapping
    {
        public IEnumerable<Vocabulary> RequiredVocabularies {
            get
            {
                return new List<Vocabulary>
                {
                    Vocabularies.JnccCategory,
                    Vocabularies.JnccDomain,
                    new Vocabulary
                        {
                            Id = "http://vocab.jncc.gov.uk/publication-status",
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
                            Id = "http://vocab.jncc.gov.uk/publication-category",
                            Name = "Publication category",
                            Description = "Publication category",
                            Controlled = true,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>()
                        },
                        new Vocabulary
                        {
                            Id = "http://vocab.jncc.gov.uk/jncc-report-series-number",
                            Name = "JNCC Report series number",
                            Description = "JNCC Report series number",
                            Controlled = false,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>()
                        },
                        new Vocabulary
                        {
                            Id = "http://vocab.jncc.gov.uk/NHBS",
                            Name = "NHBS Numbers",
                            Description = "NHBS Number",
                            Controlled = false,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>()
                        },
                        new Vocabulary
                        {
                            Id = "http://vocab.jncc.gov.uk/ISBN",
                            Name = "ISBN Numbers",
                            Description = "ISBN Numbers",
                            Controlled = false,
                            Publishable = true,
                            PublicationDate = "2015",
                            Keywords = new List<VocabularyKeyword>()
                        },
                        new Vocabulary
                        {
                            Id = "http://vocab.jncc.gov.uk/ISSN",
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
                        sb.AppendLine("#Short Summary");
                        sb.AppendLine(row.GetField("ShortSummary"));
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

                    return sb.ToString();
                });
                //Invalid dates handled by exporter - go to comments field with note
                Map(m => m.DatasetReferenceDate)
                    .ConvertUsing(
                        row =>
                            RecordValidator.IsValidDate(row.GetField("PublicationDate"))
                                ? row.GetField("PublicationDate")
                                : String.Empty);

                Map(m => m.Keywords).ConvertUsing(GetKeywords);
                Map(m => m.ResourceLocator).ConvertUsing(row => "http://some/example/public/location");
                Map(m => m.DataFormat).ConvertUsing(row => "Documents");
                Map(m => m.ResourceType).ConvertUsing(row => "publication");
            }


            private List<MetadataKeyword> GetKeywords(ICsvReaderRow row)
            {
                var keywords = new List<MetadataKeyword>();

                keywords.AddRange(ParsePageKeywords(row.GetField("Keywords")));

                AddKeyword(keywords, "http://vocab.jncc.gov.uk/NHBS", row.GetField("NhbsNumber"));
                AddKeyword(keywords, "http://vocab.jncc.gov.uk/ISBN", row.GetField("IsbnNumber"));
                AddKeyword(keywords, "http://vocab.jncc.gov.uk/ISSN", row.GetField("IssnNumber"));
                AddKeyword(keywords, "http://vocab.jncc.gov.uk/jncc-report-series-number", row.GetField("JnccReportSeriesNumber"));

                if (row.GetField("Free") == "1")
                {
                    AddKeyword(keywords, "http://vocab.jncc.gov.uk/publication-status", "Free");
                }

                if (row.GetField("Discontinued") == "1")
                {
                    AddKeyword(keywords, "http://vocab.jncc.gov.uk/publication-status", "Discontinued");
                }

                // not sure yet how to categorise publications
                AddKeyword(keywords, "http://vocab.jncc.gov.uk/jncc-domain", "to do!");
                AddKeyword(keywords, "http://vocab.jncc.gov.uk/jncc-category", "Publications");

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

            private IEnumerable<MetadataKeyword> ParsePageKeywords(string input)
            {

                if (input.IsBlank()) return new List<MetadataKeyword>();

                return (from m in Regex.Matches(input, @"\{(.*?)\}").Cast<Match>()
                        let keyword = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1).First().Split(',')
                        from k in keyword
                        where k.IsNotBlank()
                        select new MetadataKeyword
                        {
                            Vocab = "http://vocab.jncc.gov.uk/publication-category",
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
            imported.Count().Should().Be(863);
        }
    }

}
