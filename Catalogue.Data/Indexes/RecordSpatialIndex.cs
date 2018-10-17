using Catalogue.Data.Model;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Indexes.Spatial;
using System.Linq;

namespace Catalogue.Data.Indexes
{
    //public class RecordSpatialIndex : AbstractIndexCreationTask<Record>
    //{
    //    public RecordSpatialIndex()
    //    {
    //        Map = records => from r in records
    //                         select new
    //                         {
    //                             __ = Spatial(FieldNames.Spatial, r.Wkt, SpatialSearchStrategy.QuadPrefixTree, 16)
    //                         };
    //    }
    //}
}