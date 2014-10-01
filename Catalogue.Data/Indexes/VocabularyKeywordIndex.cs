using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Analyzers;
using Catalogue.Gemini.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class VocabularyKeywordIndex : AbstractIndexCreationTask<Vocabulary, VocabularyKeywordIndex.Result>
    {

        public class Result
        {
            public string Key { get; set; }
            public string Vocab { get; set; }
            public string Value { get; set; }
            public string ValueN { get; set; }
        }

        public VocabularyKeywordIndex()
        {
            // we use a separate field for custom ngram search because it's of limited length
            // and we want to always be able to match the full keyword!

            Map = vocabularies => from vocab in vocabularies
                                 from keyword in vocab.Values
                                 select new 
                                 {
                                     Key = vocab.Id + "::" + keyword, // make a unique key field
                                     Vocab = vocab.Id,
                                     Value = keyword,
                                     ValueN = keyword,
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
            Stores.Add(x => x.Value, FieldStorage.Yes);
            Analyze(x => x.ValueN, typeof(CustomKeywordAnalyzer).AssemblyQualifiedName); 
        }
    }
}
