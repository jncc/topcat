﻿using System;
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

            config.RegisterClassMap<RecordMap>();
            config.RegisterClassMap<MetadataMap>();
        }

        public class RecordMap : CsvClassMap<Record>
        {
            public override void CreateMap()
            {
                Map(m => m.Notes).Ignore();
                References<MetadataMap>(m => m.Gemini);
            }
        }

        public class MetadataMap : CsvClassMap<Metadata>
        {
            public override void CreateMap()
            {
                Map(m => m.Title);
                Map(m => m.Abstract);
            }
        }
    }
}
