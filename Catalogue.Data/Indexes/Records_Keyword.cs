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
    public class Records_Keyword : AbstractIndexCreationTask<Record>
    {
        public Records_Keyword()
        {
            //Map = records => from doc in records
            //                 from docGeminiKeywordsItem in ((IEnumerable<MetadataKeyword>) doc.Gemini.Keywords).DefaultIfEmpty()
            //                 select new {Gemini_Keyword = docGeminiKeywordsItem};

            //Stores.Add(x => x.Gemini.Keywords., FieldStorage.Yes);

            Map = records => from doc in records
                             from docGeminiKeywordsItem in (doc.Gemini.Keywords).DefaultIfEmpty()
                             select
                                 new
                                     {
                                         Vocab = docGeminiKeywordsItem.Vocab,
                                         Value = docGeminiKeywordsItem.Value
                                     };
        }
    }
}
