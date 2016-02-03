using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class RecordsWithDuplicateTitleCheckerIndex : AbstractIndexCreationTask<Record, RecordsWithDuplicateTitleCheckerIndex.Result>
    {
        public class Result
        {
             public string Title { get; set; }
             public int Count { get; set; }
        }

        public RecordsWithDuplicateTitleCheckerIndex()
        {
            Map = records => from r in records
                             select new
                             {
                                 Title = r.Gemini.Title,
                                 Count = 1
                             };
            Reduce = results => from r in results
                                group r by r.Title into g
                                select new
                                {
                                    Title = g.Key,
                                    Count = g.Sum(r => r.Count)
                                };
        }
    }
}
