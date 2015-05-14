using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Export;
using Catalogue.Gemini.Model;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Catalogue.Data.Import.Mappings
{
    /// <summary>
    /// Supports a standard Topcat tab-separated record format (as created by the exporter).
    /// </summary>
    public class TopcatMapping : IMapping
    {
        public IEnumerable<Vocabulary> RequiredVocabularies { get; private set; }
        
        public void Apply(CsvConfiguration config)
        {
            ApplyStandardTopcatCsvConfiguration(config);
            // do nothing else! for this import mapping we're going to use defaults
        }

        public static CsvConfiguration ApplyStandardTopcatCsvConfiguration(CsvConfiguration config)
        {
            config.Delimiter = "\t";
            config.PrefixReferenceHeaders = true;
            TypeConverterFactory.AddConverter<List<MetadataKeyword>>(new Exporter.MetadataKeywordConverter());
            TypeConverterFactory.AddConverter<List<Extent>>(new Exporter.ExtentListConverter());

            return config;
        }
    }
}
