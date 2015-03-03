using System.Linq;
using Catalogue.Data.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class RecordSpatialIndex : AbstractIndexCreationTask<Record>
    {
        public RecordSpatialIndex()
        {
            Map = records => from r in records
                             select new
                             {
                                 __ = SpatialGenerate(FieldNames.Spatial, r.Wkt, SpatialSearchStrategy.QuadPrefixTree, 16)
                             };
        }
    }
}
