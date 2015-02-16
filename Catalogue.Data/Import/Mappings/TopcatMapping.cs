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
    /// Supports the standard Topcat tab-separated record format (as created by the exporter).
    /// </summary>
    public class TopcatMapping : IMapping
    {
        public IEnumerable<Vocabulary> RequiredVocabularies { get; private set; }
        
        public void Apply(CsvConfiguration config)
        {
            config.Delimiter = "\t";
            TypeConverterFactory.AddConverter<List<MetadataKeyword>>(new Exporter.MetadataKeywordConverter());
            TypeConverterFactory.AddConverter<List<Extent>>(new Exporter.ExtentListConverter());

            // do nothing! for this import mapping we're going to use defaults
        }
    }
}
