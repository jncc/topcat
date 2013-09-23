using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;

namespace Catalogue.Data.Import.Formats
{
    public class DefaultFormat : IFormat
    {
        public void Configure(CsvConfiguration config)
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
            }
        }
    }
}
