using System;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;

namespace Catalogue.Import.Formats
{
    public class DefaultFormat : IFormat
    {
        public void Configure(CsvConfiguration config)
        {
            // see http://joshclose.github.io/CsvHelper/

            config.RegisterClassMap<DefaultRecordMap>();
            //config.RegisterClassMap<DefaultMetadataMap>();
        }
    }

    public class DefaultRecordMap : CsvClassMap<Record>
    {
        public override void CreateMap()
        {
            Map(m => m.Notes).Name("Notes");
        }
    }

    public class DefaultMetadataMap : CsvClassMap<Metadata>
    {
        public override void CreateMap()
        {
            Map(m => m.Abstract).Name("Abstract");
        }
    }

}
