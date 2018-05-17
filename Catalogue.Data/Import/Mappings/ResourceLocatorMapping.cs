using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;
using System.Collections.Generic;

namespace Catalogue.Data.Import.Mappings
{

    public class ResourceLocatorMapping : IMapping
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
                Map(m => m.ResourceLocator).Field("RESOURCE_URL");
            }
        }

        public sealed class RecordMap : CsvClassMap<Record>
        {
            public RecordMap()
            {
                Map(m => m.Id).Field("TOPCAT_ID");
                References<GeminiMap>(m => m.Gemini);
            }
        }
    }
}
