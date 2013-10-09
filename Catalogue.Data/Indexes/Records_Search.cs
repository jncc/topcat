using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
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
                                     Abstract = record.Gemini.Abstract
                                 };

            Index(x => x.Title, FieldIndexing.Analyzed);
            Stores.Add(x => x.Title, FieldStorage.Yes);
            TermVector(x => x.Title, FieldTermVector.WithPositionsAndOffsets);

            Index(x => x.Abstract, FieldIndexing.Analyzed);
            Stores.Add(x => x.Abstract, FieldStorage.Yes);
            TermVector(x => x.Abstract, FieldTermVector.WithPositionsAndOffsets);
        }
    }
}
