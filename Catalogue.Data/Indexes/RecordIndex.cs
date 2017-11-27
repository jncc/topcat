using System;
using System.Linq;
using Catalogue.Data.Analyzers;
using Catalogue.Data.Model;
using Catalogue.Gemini.DataFormats;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    /// <summary>
    /// This is the primary / most important index, used for searching / exporting records.
    /// </summary>
    public class RecordIndex : AbstractIndexCreationTask<Record, RecordIndex.Result>
    {
        public class Result
        {
            public string   Title        { get; set; }
            public string   TitleN       { get; set; }
            public string   Abstract     { get; set; }
            public string   AbstractN    { get; set; }
            public string[] Keywords     { get; set; }
            public string[] KeywordsN    { get; set; }
            public DateTime MetadataDate { get; set; }
            public string   DataFormat   { get; set; }
            public string   Target       { get; set; }
            public string   Manager  { get; set; }
        }

        public RecordIndex()
        {
            Map = records => from record in records
                             select new
                                 {
                                     // redundant explicit property names are actually needed - index doesn't work properly without!
                                     Title = record.Gemini.Title,
                                     Gemini_Title = record.Gemini.Title,
                                     TitleN = record.Gemini.Title, 
                                     Abstract = record.Gemini.Abstract,
                                     AbstractN = record.Gemini.Abstract,
                                     Keywords = record.Gemini.Keywords.Select(k => k.Vocab + "/" + k.Value), // for filtering exactly on keywords
                                     KeywordsN = record.Gemini.Keywords.Select(k => k.Value), // for full-text search matching on keywords
                                     MetadataDate = record.Gemini.MetadataDate,
                                     DataFormat = record.Gemini.DataFormat,
                                     Gemini_DatasetReferenceDate = record.Gemini.DatasetReferenceDate,
                                     Manager = record.Manager.DisplayName,
                             };

            // store and analyse the Title field
            Analyze(x => x.Title, typeof(StemAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Title, FieldStorage.Yes);
            TermVector(x => x.Title, FieldTermVector.WithPositionsAndOffsets);
            Analyze(x => x.TitleN, typeof(NGramAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.TitleN, FieldStorage.Yes);
            TermVector(x => x.TitleN, FieldTermVector.WithPositionsAndOffsets);

            // store and analyse the Abstract field
            Analyze(x => x.Abstract, typeof(StemAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Abstract, FieldStorage.Yes);
            TermVector(x => x.Abstract, FieldTermVector.WithPositionsAndOffsets);
            Analyze(x => x.AbstractN, typeof(NGramAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.AbstractN, FieldStorage.Yes);
            TermVector(x => x.AbstractN, FieldTermVector.WithPositionsAndOffsets);

            // store and analyse the Keywords field for full-text search
            Analyze(x => x.KeywordsN, typeof(NGramAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.KeywordsN, FieldStorage.Yes);
            TermVector(x => x.KeywordsN, FieldTermVector.WithPositionsAndOffsets);

            // store and analyse the Manager DisplayName field
            Analyze(x => x.Manager, typeof(SimpleAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Manager, FieldStorage.Yes);
            TermVector(x => x.Manager, FieldTermVector.WithPositionsAndOffsets);
        }
    }
}
