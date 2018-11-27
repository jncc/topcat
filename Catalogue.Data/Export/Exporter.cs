using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using CsvHelper;
using CsvHelper.TypeConversion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration;

namespace Catalogue.Data.Export
{
    public class Exporter
    {
        public void Export(IEnumerable<Record> records, TextWriter writer, string delimiter)
        {
            var csv = new CsvWriter(writer);

            TopcatMapping.ApplyStandardTopcatConfiguration(csv.Configuration, delimiter);

            csv.WriteRecords(records);
        }


        public void ExportHeader(TextWriter writer, string delimiter)
        {
            var csv = new CsvWriter(writer);

            TopcatMapping.ApplyStandardTopcatConfiguration(csv.Configuration, delimiter);

            csv.WriteHeader<Record>();
        }

        public void ExportRecord(Record record, TextWriter writer, string delimiter)
        {
            var csv = new CsvWriter(writer);

            TopcatMapping.ApplyStandardTopcatConfiguration(csv.Configuration, delimiter);

            csv.WriteRecord(record);
        }

        public class MetadataKeywordConverter : DefaultTypeConverter
        {
            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                var v = (List<MetadataKeyword>)value;
                return JsonConvert.SerializeObject(v);
            }

            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                return JsonConvert.DeserializeObject(text, typeof(List<MetadataKeyword>));
            }
        }

        public class ExtentListConverter : DefaultTypeConverter
        {
            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                var v = (List<Extent>)value;
                return JsonConvert.SerializeObject(v);
            }
        }
    }


}
