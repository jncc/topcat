using System.Linq;
using Catalogue.Data.Analyzers;
using Catalogue.Data.Model;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using Raven.Database.Indexing;

namespace Catalogue.Data.Indexes
{
    /// <summary>
    /// This index just makes a list of distinct keywords (vocab/value pairs).
    /// </summary>
    public class KeywordsSearchIndex : AbstractIndexCreationTask<Record, KeywordsSearchIndex.Result>
    {
        public class Result
        {
            public string Key { get; set; }
            public string Vocab { get; set; }
            public string Value { get; set; }
            public string ValueN { get; set; }
        }

        public KeywordsSearchIndex()
        {
            // we use a separate field for custom ngram search because it's of limited length
            // and we want to always be able to match the full keyword!

            Map = records => from record in records
                             from k in record.Gemini.Keywords
                             select new
                             {
                                 Key = k.Vocab + "::" + k.Value, // make a unique key field
                                 Vocab = k.Vocab,
                                 Value = k.Value,
                                 ValueN = k.Value,
                             };

            Reduce = xs => from x in xs
                           group x by x.Key into g
                           select new
                           {
                               Key = g.Key,
                               Vocab = g.First().Vocab,
                               Value = g.First().Value,
                               ValueN = g.First().Value,
                           };

//            Analyze(x => x.Vocab, typeof(LowerCaseKeywordAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Vocab, FieldStorage.Yes);
//            Analyze(x => x.Value, typeof(LowerCaseKeywordAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Value, FieldStorage.Yes);
            Analyze(x => x.ValueN, typeof(CustomKeywordAnalyzer).AssemblyQualifiedName); 
          
        }
    }
}
