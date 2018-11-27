using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Export;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Data.Exchange
{
    /// <summary>
    /// TODO
    /// </summary>
    public class TopcatExchangeFormat : IWriterMapping
    {
        public IEnumerable<Vocabulary> RequiredVocabularies
        {
            get { throw new NotImplementedException(); }
        }

        public void Apply(IWriterConfiguration config)
        {
            config.Delimiter = "\t";
            config.RegisterClassMap<MyRecordMap>();
            config.RegisterClassMap<MyMetadataMap>();
            
            config.TypeConverterCache.AddConverter<List<MetadataKeyword>>(new Exporter.MetadataKeywordConverter());
            config.TypeConverterCache.AddConverter<List<Extent>>(new Exporter.ExtentListConverter());

        }

        public sealed class MyRecordMap : ClassMap<Record>
        {
            public MyRecordMap()
            {
                this.AutoMap();
                References<MyMetadataMap>(m => m.Gemini);
            }
        }

        public sealed class MyMetadataMap : ClassMap<Metadata>
        {
            readonly List<string> extentCols = new List<string> { "Extent1", "Extent2", "Extent3", "Extent4", "Extent5" }; 

            public MyMetadataMap()
            {
                this.AutoMap();
                //Map(m => m.Extent).Flatten()
            }
             
        }
    }

    [Explicit]
    class topcat_exchange_format_tests
    {
        [Test]
        public void blah()
        {
            //var stream = new MemoryStream();
            var writer = new StringWriter();
            var csv = new CsvWriter(writer);

            new TopcatExchangeFormat().Apply(csv.Configuration);

            var example = new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Example().With(m => m.Extent.Add(new Extent {Value = "Nottingam"}))
            };
            var records = new [] { example };
            
            csv.WriteRecords(records);
            foreach (var record in records)
            {
                csv.WriteRecord(record);
                csv.WriteField("blah");
                csv.NextRecord();
            }

            string result = writer.ToString();
            result.Should().Contain("blah");
        }
    }
}
