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
        public override string IndexName
        {
            get { return "KeywordsIndex"; }
        }

        public KeywordsIndex()
        {
            Map = records => from record in records
                from keyword in record.Gemini.Keywords
                select new {
                    Value = keyword.Value,
                    Vocab = keyword.Vocab
                };
            Store(x => x.Keyword.Value, FieldStorage.Yes);
            Store(x => x.Keyword.Vocab, FieldStorage.Yes);
        }


        public class Result
        {
            public Keyword Keyword { get; set; }
        }
    }
}
