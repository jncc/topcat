using System.Linq;
using Catalogue.Data.Analyzers;
using Catalogue.Data.Model;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class KeywordsSearchIndex : AbstractIndexCreationTask<Record, KeywordsSearchIndex.Result>
    {
        public class Result
        {
            public string Key { get; set; }
            public string Vocab { get; set; }
            public string Value { get; set; }
        }

        public KeywordsSearchIndex()
        {
            // make an index of distinct keywords (vocab/value pairs)

            Map = records => from record in records
                             from k in record.Gemini.Keywords
                             select new
                             {
                                 Key = k.Vocab + "::" + k.Value, // make a unique key field
                                 Vocab = k.Vocab,
                                 Value = k.Value,
                             };

            Reduce = xs => from x in xs
                           group x by x.Key into g
                           select new
                           {
                               Key = g.Key,
                               Vocab = g.First().Vocab,
                               Value = g.First().Value,
                           };

            Analyze(x => x.Value, typeof(CustomKeywordAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Value, FieldStorage.Yes);

            Analyze(x => x.Value, typeof(KeywordAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Vocab, FieldStorage.Yes);
        }
    }
}
