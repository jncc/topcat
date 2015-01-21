using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
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
            Map(m => m.AdditionalInformationSource).Name("Author");

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
