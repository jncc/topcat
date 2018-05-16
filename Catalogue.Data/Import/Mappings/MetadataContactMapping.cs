using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;
using System.Collections.Generic;

namespace Catalogue.Data.Import.Mappings
{

    public class MetadataContactMapping : IMapping
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

        public sealed class ManagerMap : CsvClassMap<UserInfo>
        {
            public ManagerMap()
            {
                Map(m => m.DisplayName).Field("Manager.DisplayName");
            }
        }

        public sealed class MetadataPointOfContactMap : CsvClassMap<ResponsibleParty>
        {
            public MetadataPointOfContactMap()
            {
                Map(m => m.Name).Field("MetadataPointOfContact.Name");
                Map(m => m.Email).Field("MetadataPointOfContact.Email");
            }
        }

        public sealed class GeminiMap : CsvClassMap<Metadata>
        {
            public GeminiMap()
            {
                References<MetadataPointOfContactMap>(m => m.MetadataPointOfContact);
            }
        }

        public sealed class RecordMap : CsvClassMap<Record>
        {
            public RecordMap()
            {
                Map(m => m.Id);
                References<GeminiMap>(m => m.Gemini);
                References<ManagerMap>(m => m.Manager);
            }
        }
    }
}
