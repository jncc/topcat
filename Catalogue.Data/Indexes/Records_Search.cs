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
    public class Records_Search : AbstractIndexCreationTask<Record, Metadata>
    {
        public Records_Search()
        {
            Map = records => from record in records
                             select new
                                 {
                                     // redundant explicit property names are actually needed - index doesn't work properly without!
                                     Title = record.Gemini.Title,
                                     Lineage = record.Gemini.Title, 
                                     Abstract = record.Gemini.Abstract
                                 };

            Analyze(x => x.Title, typeof(StemAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Title, FieldStorage.Yes);
            TermVector(x => x.Title, FieldTermVector.WithPositionsAndOffsets);

            Analyze(x => x.Lineage, typeof(NGramAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Lineage, FieldStorage.Yes);
            TermVector(x => x.Lineage, FieldTermVector.WithPositionsAndOffsets);


            Analyze(x => x.Abstract, typeof(NGramAnalyzer).AssemblyQualifiedName);
            Stores.Add(x => x.Abstract, FieldStorage.Yes);
            TermVector(x => x.Abstract, FieldTermVector.WithPositionsAndOffsets);
        }
    }
}
