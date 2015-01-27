using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using CsvHelper;
using CsvHelper.Configuration;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;

namespace Catalogue.Data.Import.Mappings
{
    public class PubCatMapper : BaseMapper, IMapping
    {
        public void Apply(CsvConfiguration config)
        {
            config.Delimiter = "\t";
            config.QuoteAllFields = true;
            config.TrimFields = true;
            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<GeminiMap>();
            
        }
    }

    public class GeminiMap : CsvClassMap<Metadata>
    {
        public override void CreateMap()
        {
            Map(m => m.Title).Name("Title");
            Map(m => m.ResponsibleOrganisation).ConvertUsing(row =>
                {
                    string name = row.GetField("Authors");
                    string email = String.Empty;
                    string role = "Author";

                    return new ResponsibleParty {Name = name, Email = email, Role = role};
                });
            Map(m => m.Abstract).ConvertUsing(row =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(row.GetField("ParsedPageContent"));
                    sb.AppendLine();
                    sb.AppendLine("## Citation");
                    sb.AppendLine();
                    sb.AppendLine(row.GetField("Citation"));
                    sb.AppendLine();
                    sb.AppendLine(row.GetField("Comment"));

                    return sb.ToString();

                });
            //Invalid dates handled by exporter - go to comments field with note
            Map(m => m.DatasetReferenceDate).Name("PublicationDate");
            Map(m => m.Keywords).ConvertUsing(ParseKeywords);
        }


        private List<MetadataKeyword> ParseKeywords(ICsvReaderRow row)
        {
            var keywords = new List<MetadataKeyword>();
            
            keywords.AddRange(GetPageKeywords(row.GetField("Keywords")));

            AddKeyword(keywords, "http://vocab.jncc.gov.uk/NHBS",  row.GetField("NhbsNumber"));
            AddKeyword(keywords, "http://vocab.jncc.gov.uk/ISBN", row.GetField("IsbnNumber"));
            AddKeyword(keywords, "http://vocab.jncc.gov.uk/ISSN", row.GetField("IssnNumber"));
            AddKeyword(keywords, "http://vocab.jncc.gov.uk/JnccReportSeriesNumber", row.GetField("JnccReportSeriesNumber"));
            AddKeyword(keywords, "http://vocab.jncc.gov.uk/publications", row.GetField("Free"));
            AddKeyword(keywords, "http://vocab.jncc.gov.uk/publications", row.GetField("Discontinued"));

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

        private IEnumerable<MetadataKeyword> GetPageKeywords(string input)
        {
            if (input.IsBlank()) return  new List<MetadataKeyword>();

            return from m in Regex.Matches(input, @"\{(.*?)\}").Cast<Match>()
                   let keyword = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1).First().Split(',')
                   from k in keyword
                   where k.IsNotBlank()
                   select new MetadataKeyword
                       {
                           Vocab = String.Empty,
                           Value = k.Replace('"', ' ').Trim()
                       };
        }
    }



    public class RecordMap : CsvClassMap<Record>
    {
        public override void CreateMap()
        {
            Map(m => m.Path).Name("Path")

            References<GeminiMap>(m => m.Gemini);
        }
    }
    
    internal class import_runnner
    {
        [Explicit]
        [Test]
        public void RunPubcatImport()
        {
            var store = new DocumentStore();
            store.ParseConnectionString("Url=http://localhost:8888/");
            store.Initialize();

            using (IDocumentSession db = store.OpenSession())
            {
                var importer = Importer.CreateImporter<PubCatMapper>(db);
                importer.Import(@"C:\Working\pubcat.csv");
            }
        }
    }

}
