using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;
using System.Collections.Generic;

namespace Catalogue.Data.Import.Mappings
{

    public class PathMapping : IMapping
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

            config.WillThrowOnMissingField = false;
            config.TrimFields = true;
        }

        public sealed class RecordMap : CsvClassMap<Record>
        {
            public RecordMap()
            {
                Map(m => m.Id).Field("Topcat ID");
                Map(m => m.Path).Field("New internal path");
            }
        }
    }
}
