using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class RecordCountForKeywordIndex : AbstractIndexCreationTask<Record, RecordCountForKeywordIndex.Result>
    {
        public class Result
        {
            public string Key { get; set; }
            public string KeywordVocab { get; set; }
            public string KeywordValue { get; set; }
            public int RecordCount { get; set; }
        }

        /// <summary>
        ///  Counts the number of records tagged with a keyword.
        /// </summary>
        public RecordCountForKeywordIndex()
        {
            Map = records => from record in records
                             from keyword in record.Gemini.Keywords
                             select new
                             {
                                 Key = keyword.Vocab + "::" + keyword.Value, // make a unique key field
                                 KeywordVocab = keyword.Vocab,
                                 KeywordValue = keyword.Value,
                                 RecordCount = 1
                             };

            Reduce = results => from result in results
                                group result by result.Key into g
                                select new
                                {
                                    Key = g.Key,
                                    KeywordVocab = g.Select(r => r.KeywordVocab).FirstOrDefault(),
                                    KeywordValue = g.Select(r => r.KeywordValue).FirstOrDefault(),
                                    RecordCount = g.Sum(r => r.RecordCount)
                                };

            
        }
    }
}
