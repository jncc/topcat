using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using Raven.Storage.Esent.SchemaUpdates.Updates;

namespace Catalogue.Data.Indexes
{
    public class KeywordIndex : AbstractIndexCreationTask<Record>//, Keyword>
    {


        public KeywordIndex()
        {
            Map = records => from record in records
                from k in record.Gemini.Keywords
                select new {k.Value, k.Vocab};

//            Reduce = keywords => from keyword in keywords
//                group keyword by keyword.Value into g
//                select new { g.Key };


        }
    }
}
