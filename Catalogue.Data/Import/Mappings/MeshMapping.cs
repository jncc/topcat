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
            }
        }
    }
}
