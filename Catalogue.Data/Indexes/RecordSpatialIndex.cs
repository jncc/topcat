using System.Linq;
using Catalogue.Data.Model;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Indexes.Spatial;

namespace Catalogue.Data.Indexes
{
    public class RecordSpatialIndex : AbstractIndexCreationTask<Record>
    {
        public RecordSpatialIndex()
        {
            Map = records => from r in records
                             select new
                             {
// raven4
//                                 __ = SpatialGenerate(FieldNames.Spatial, r.Wkt, SpatialSearchStrategy.QuadPrefixTree, 16)
                             };
        }
    }
}
