using Catalogue.Data.Model;
using Raven.Client.Documents.Indexes;
using System.Linq;

namespace Catalogue.Data.Indexes
{
    public class RecordSpatialIndex : AbstractIndexCreationTask<Record>
    {
        public RecordSpatialIndex()
        {
            Map = records => from r in records
                             select new
                             {
                                 Shape = CreateSpatialField(r.Wkt)
                             };

            //Spatial(x => x.Shape, options => options.Geography.QuadPrefixTreeIndex(16));
        }
    }
}