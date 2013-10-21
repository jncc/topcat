using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;

namespace Catalogue.Data.Import.Mappings
{
    /// <summary>
    /// The mappings for importing the Marine Habitat (MESH) data.
    /// </summary>
    public class MeshMapping : IMapping
    {
        public void Apply(CsvConfiguration config)
        {
            // see http://joshclose.github.io/CsvHelper/

            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<MetadataMap>();
        }

        public class RecordMap : CsvClassMap<Record>
        {
            public override void CreateMap()
            {
                this.Map(m => m.Notes).Ignore();
                this.References<MetadataMap>(m => m.Gemini);
            }
        }

        public class MetadataMap : CsvClassMap<Metadata>
        {
            public override void CreateMap()
            {
                this.Map(m => m.Title);
                this.Map(m => m.Abstract);
                this.Map(m => m.Keywords).ConvertUsing(row =>
                    {
                        string input = row.GetField("Keywords");
                        return ParseMeshKeywords(input);
                    });
            }
        }

        public static List<Keyword> ParseMeshKeywords(string input)
        {
            var q = from m in Regex.Matches(input, @"\{(.*?)\}").Cast<Match>()
                    let pair = m.Groups.Cast<Group>().Select(g => g.Value).Skip(1).First().Split(',')
                    select new Keyword
                    {
                        // todo: map the source vocab IDs to "real" ones
                        VocabularyIdentifier = pair.ElementAt(0).Trim().Trim('"'),
                        Value = pair.ElementAt(1).Trim().Trim('"'),
                    };

            return q.ToList();
        }
    }
}
