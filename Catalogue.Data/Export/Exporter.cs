using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using CsvHelper;
using CsvHelper.TypeConversion;
using Newtonsoft.Json;

namespace Catalogue.Data.Export
{
    public class Exporter
    {
        public void Export(IEnumerable<Record> records, TextWriter writer)
        {
            var csv = new CsvWriter(writer);

            TopcatMapping.ApplyStandardTopcatCsvConfiguration(csv.Configuration);

            csv.WriteRecords(records);
        }


        public class MetadataKeywordConverter : DefaultTypeConverter
        {
            public override bool CanConvertTo(Type type)
            {
                return type == typeof(string);
            }

            public override string ConvertToString(TypeConverterOptions options, object value)
            {
                var v = (List<MetadataKeyword>)value;
                return JsonConvert.SerializeObject(v);
            }

            public override bool CanConvertFrom(Type type)
            {
                return type == typeof(string);
            }

            public override object ConvertFromString(TypeConverterOptions options, string text)
            {
                return JsonConvert.DeserializeObject(text, typeof(List<MetadataKeyword>));
            }
        }

        public class ExtentListConverter : DefaultTypeConverter
        {
            public override bool CanConvertTo(Type type)
            {
                return type == typeof(string);
            }

            public override string ConvertToString(TypeConverterOptions options, object value)
            {
                var v = (List<Extent>)value;
                return JsonConvert.SerializeObject(v);
            }
        }
    }

}
