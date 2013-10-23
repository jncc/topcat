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
    public class Keywords_Search : AbstractIndexCreationTask<Record, Keyword>
    {
        public Keywords_Search()
        {
            Map = records => from record in records
                             from k in record.Gemini.Keywords
                             select new
                             {
                                 Vocab = k.Vocab,
                                 Value = k.Value,
                             };

//            Analyze(x => x.Value, typeof(NGramAnalyzer).AssemblyQualifiedName);
//            Stores.Add(x => x.Value, FieldStorage.Yes);
        }
    }
}
