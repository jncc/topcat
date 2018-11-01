﻿using System.Linq;
using Catalogue.Data.Model;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Data.Indexes
{
    /// <summary>
    /// This index just makes a list of distinct keywords (vocab/value pairs).
    /// </summary>
    public class RecordKeywordIndex : AbstractIndexCreationTask<Record, RecordKeywordIndex.Result>
    {
        public class Result
        {
            public string Key { get; set; }
            public string Vocab { get; set; }
            public string Value { get; set; }
            public string ValueN { get; set; }
        }

        public RecordKeywordIndex()
        {
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

            Stores.Add(x => x.Vocab, FieldStorage.Yes);
            Analyze(x => x.Value, typeof(StandardAnalyzer).AssemblyQualifiedName);

            Stores.Add(x => x.Value, FieldStorage.Yes);
            Analyze(x => x.ValueN, "Catalogue.Data.Analyzers.CustomKeywordAnalyzer, Catalogue.Data.Analyzers");
        }
    }
}
