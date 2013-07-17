using System.Linq;
using Catalogue.Data.Model;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Catalogue.Data.Indexes
{
    public class Items_SpatialIndex : AbstractIndexCreationTask<Item>
    {
        public Items_SpatialIndex()
        {
            Map = items => from e in items
                           select new
                           {
                               __ = SpatialGenerate(FieldNames.Spatial, e.Wkt, SpatialSearchStrategy.QuadPrefixTree, 16)
                           };
        }
    }
}
