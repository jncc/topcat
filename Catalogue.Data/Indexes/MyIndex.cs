using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Data.Indexes
{
    public class MyIndex : AbstractIndexCreationTask<Record, MyIndex.Result>
    {
        public class Result
        {
            public string[] Keywords { get; set; }
        }

        public MyIndex()
        {
            Map = records => from record in records
                             select new
                             {
                                 Keywords = record.Gemini.Keywords.Select(k => k.Vocab + "/" + k.Value)
                             };
        }
    }
}
