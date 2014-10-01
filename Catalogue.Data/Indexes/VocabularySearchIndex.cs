using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Analyzers;
using Catalogue.Data.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    /// <summary>
    /// This index just makes a list of distinct keywords (vocab/value pairs).
    /// </summary>
    public class VocabularySearchIndex : AbstractIndexCreationTask<Record, Indexes.VocabularySearchIndex.Result>
    {
        public class Result
        {
            public string Vocab { get; set; }
            public string VocabN { get; set; }
        }

        public VocabularySearchIndex()
        {

            Map = records => from record in records
                             from k in record.Gemini.Keywords
                             select new
                             {
                                 Vocab = k.Vocab,
                                 VocabN = k.Vocab

                             };

            Reduce = xs => from x in xs
                           group x by x.Vocab into g
                           select new
                           {
                               Vocab = g.First().Vocab,
                               VocabN = g.First().Vocab
                           };

            Stores.Add(x => x.Vocab, FieldStorage.Yes);
            Analyze(x => x.VocabN, typeof(CustomKeywordAnalyzer).AssemblyQualifiedName); 
          
        }
    }
}
