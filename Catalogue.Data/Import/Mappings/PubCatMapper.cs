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

namespace Catalogue.Data.Import.Mappings
{
    public class PubCatMapper : BaseMapper, IMapping
    {
        public void Apply(CsvConfiguration config)
        {
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
                    string name = row.GetField("Author");
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
            keywords.Add(GetNhbsNumber(row.GetField("NhbsNumber")));
            keywords.Add(GetIsbnNumber(row.GetField("IsbnNumber")));
            keywords.Add(GetIssnNumber(row.GetField("IssnNumber")));
            keywords.Add(GetJnccReportSeriesNumber(row.GetField("JnccReportSeriesNumber")));
            keywords.Add(GetFreePublicationIndicator(row.GetField("Free")));
            keywords.Add(GetDiscontinuedPublicationIndicator(row.GetField("Discontinued")));

            return keywords;
        }

        private MetadataKeyword GetDiscontinuedPublicationIndicator(string discontinuedIndicator)
        {
            if (discontinuedIndicator.IsBlank()) return null;
            if (discontinuedIndicator.ToLower() != "1") return null;

            return new MetadataKeyword()
            {
                Value = "Discontinued",
                Vocab = "http://vocab.jncc.gov.uk/publications"
            };
        }

        private MetadataKeyword GetFreePublicationIndicator(string freeIndicator)
        {
            if (freeIndicator.IsBlank()) return null;
            if (freeIndicator.ToLower() != "1") return null;

            return new MetadataKeyword()
            {
                Value = "Free",
                Vocab = "http://vocab.jncc.gov.uk/publications"
            };
        }


        private MetadataKeyword GetJnccReportSeriesNumber(string jnccReportSeriesNo)
        {
            if (jnccReportSeriesNo.IsBlank()) return null;

            return new MetadataKeyword()
            {
                Value = jnccReportSeriesNo,
                Vocab = "http://vocab.jncc.gov.uk/JnccReportSeriesNumber"
            };
        }

        private MetadataKeyword GetIssnNumber(string issnNo)
        {
            if (issnNo.IsBlank()) return null;

            return new MetadataKeyword()
            {
                Value = issnNo,
                Vocab = "http://vocab.jncc.gov.uk/ISSN"
            };
        }

        private MetadataKeyword GetIsbnNumber(string isbnNo)
        {
            if (isbnNo.IsBlank()) return null;

            return new MetadataKeyword()
            {
                Value = isbnNo,
                Vocab = "http://vocab.jncc.gov.uk/ISBN"
            };
        }

        private MetadataKeyword GetNhbsNumber(string nhbsNo)
        {
            if (nhbsNo.IsBlank()) return null;

            return new MetadataKeyword()
                {
                    Value = nhbsNo,
                    Vocab = "http://vocab.jncc.gov.uk/NHBS"
                };
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
            throw new NotImplementedException();
        }
    }
}
    ;