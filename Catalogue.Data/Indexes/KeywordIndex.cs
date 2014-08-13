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
    public class KeywordsIndex : AbstractIndexCreationTask<Record, KeywordsIndex.Result>
    {
        public KeywordsIndex()
        {
            Map = records => from record in records
                from keyword in record.Gemini.Keywords
                select new
                {
                    Keyword = keyword
                };
        }


        public class Result
        {
            public Keyword Keyword { get; set; }
        }
    }
}
