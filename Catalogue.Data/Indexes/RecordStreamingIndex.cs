using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Client.Documents.Indexes;

namespace Catalogue.Data.Indexes
{
    /// <summary>
    /// Gets all the records, so we can stream them. 
    /// The Raven streaming API requires a static index, so here it is. (Maybe there's a neater way of doing it.)
    /// </summary>
    public class RecordStreamingIndex : AbstractIndexCreationTask<Record>
    {
        public RecordStreamingIndex()
        {
            Map = records => from r in records
                             select new
                             {
                                 Id = r.Id,
                             };
        }
    }
}
