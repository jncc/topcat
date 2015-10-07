using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class JnccCategoriesIndex : AbstractIndexCreationTask<Record, JnccCategoriesIndex.Result>
    {
        public class Result
        {
            public string CategoryName { get; set; }
            public int RecordCount { get; set; }
        }

        public JnccCategoriesIndex()
        {
            Map = records => from record in records
                             from keyword in record.Gemini.Keywords
                             where keyword.Vocab == "http://vocab.jncc.gov.uk/jncc-category"
                             select new
                             {
                                 // todo need to account for records with multiple categories
                                 CategoryName = keyword.Value,
                                 RecordCount = 1
                             };

            Reduce = results => from result in results
                                group result by result.CategoryName into g
                                select new
                                {
                                    CategoryName = g.Key,
                                    RecordCount = g.Sum(r => r.RecordCount)
                                };

            
        }
    }
}
