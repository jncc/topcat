using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Analyzers;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
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
            Map = records => from record in records
                             from k in record.Gemini.Keywords
                             select new
                             {
                                 Key = k.Vocab + ":::" + k.Value,
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

//            Analyze(x => x.Value, typeof(NGramAnalyzer).AssemblyQualifiedName);
//            Stores.Add(x => x.Value, FieldStorage.Yes);
        }
    }
}
