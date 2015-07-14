using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class RecordPathStreamingIndex : AbstractIndexCreationTask<Record>
    {
        public RecordPathStreamingIndex()
        {
            Map = records => from r in records
                             select new
                             {
                                 Id = r.Id,
                                 //Path = r.Path
                             };
        }
    }
}
