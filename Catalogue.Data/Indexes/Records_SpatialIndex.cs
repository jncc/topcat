using System.Linq;
using Catalogue.Data.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class Records_SpatialIndex : AbstractIndexCreationTask<Record>
    {
        public Records_SpatialIndex()
        {
            Map = records => from r in records
                             select new
                             {
                                 __ = SpatialGenerate(FieldNames.Spatial, r.Wkt, SpatialSearchStrategy.QuadPrefixTree, 16)
                             };
        }
    }
}
