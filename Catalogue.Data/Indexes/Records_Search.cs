using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Analyzers;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Lucene.Net.Analysis.Standard;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class Records_Search : AbstractIndexCreationTask<Record, Records_Search.Shape>
    {
        public class Shape
        {
            public string Title { get; set; }
            public string TitleN { get; set; }
            public string Abstract { get; set; }
            public string AbstractN { get; set; }
        }

        public Records_Search()
        {
            Map = records => from record in records
                             select new
                                 {
                                     // redundant explicit property names are actually needed - index doesn't work properly without!
                                     Title = record.Gemini.Title,
                                     TitleN = record.Gemini.Title, 
                                     Abstract = record.Gemini.Abstract,
                                     AbstractN = record.Gemini.Abstract,
//                                     Gemini_Keywords_Value = (
//                                         from docGeminiKeywordsItem in ((IEnumerable<dynamic>)record.Gemini.Keywords).DefaultIfEmpty()
//                                         select docGeminiKeywordsItem.Value).ToArray(),
//                                     Gemini_Keywords_Vocab = (
//                                         from docGeminiKeywordsItem in ((IEnumerable<dynamic>)record.Gemini.Keywords).DefaultIfEmpty()
//                                         select docGeminiKeywordsItem.Vocab).ToArray()
                                 };

            Analyze(x => x.Title, typeof(StemAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Title, FieldStorage.Yes);
            TermVector(x => x.Title, FieldTermVector.WithPositionsAndOffsets);

            Analyze(x => x.TitleN, typeof(NGramAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.TitleN, FieldStorage.Yes);
            TermVector(x => x.TitleN, FieldTermVector.WithPositionsAndOffsets);


            Analyze(x => x.Abstract, typeof(StemAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Abstract, FieldStorage.Yes);
            TermVector(x => x.Abstract, FieldTermVector.WithPositionsAndOffsets);

            Analyze(x => x.AbstractN, typeof(NGramAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.AbstractN, FieldStorage.Yes);
            TermVector(x => x.AbstractN, FieldTermVector.WithPositionsAndOffsets);
        }
    }
}
