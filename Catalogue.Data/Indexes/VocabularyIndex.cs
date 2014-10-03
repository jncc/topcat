﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Analyzers;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class VocabularyIndex : AbstractIndexCreationTask<Vocabulary, Indexes.VocabularyIndex.Result>
    {
                public class Result
        {
            public string Vocab { get; set; }
            public string VocabN { get; set; }
        }

                public VocabularyIndex()
        {

            Map = vocabularies => from v in vocabularies
                                 select new
                                 {
                                     Vocab = v.Id,
                                     VocabN = v.Id

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
