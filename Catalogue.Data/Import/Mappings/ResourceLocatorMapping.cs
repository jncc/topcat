using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;
using System.Collections.Generic;

namespace Catalogue.Data.Import.Mappings
{

    public class ResourceLocatorMapping : IReaderMapping
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

        public void Apply(IReaderConfiguration config)
        {
            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<GeminiMap>();

            config.MissingFieldFound = null;
            config.TrimOptions = TrimOptions.Trim;
        }

        public sealed class GeminiMap : ClassMap<Metadata>
        {
            public GeminiMap()
            {
                Map(m => m.ResourceLocator).Name("RESOURCE_URL");
            }
        }

        public sealed class RecordMap : ClassMap<Record>
        {
            public RecordMap()
            {
                Map(m => m.Id).Name("TOPCAT_ID");
                References<GeminiMap>(m => m.Gemini);
            }
        }
    }
}
