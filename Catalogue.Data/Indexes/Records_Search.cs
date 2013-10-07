using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class Records_Search : AbstractIndexCreationTask<Record, Records_Search.ReduceResult>
    {
        public class ReduceResult
        {
//            public Guid Id { get; set; }
            public string Title { get; set; }
//            public string Abstract { get; set; }
        }

        public Records_Search()
        {
            Map = records => from record in records
                             let t  = record.Gemini.Title
                             select new
                                 {
//                                     record.Id,
                                     Title = t,
//                                     record.Gemini.Abstract,
                                 };

            Index(x => x.Title, FieldIndexing.Analyzed);
//            Stores.Add(x => x.Id, FieldStorage.Yes);
//            Stores.Add(x => x.Title, FieldStorage.Yes);
//            Stores.Add(x => x.Abstract, FieldStorage.Yes);
        }
    }
}
