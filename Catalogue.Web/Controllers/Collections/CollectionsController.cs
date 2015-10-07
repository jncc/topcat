using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.Collections
{
    public class CollectionsController : ApiController
    {
        readonly IDocumentSession db;

        public CollectionsController(IDocumentSession db)
        {
            this.db = db;
        }

        public List<CollectionOutputModel> Get()
        {
            var vocab = db.Load<Vocabulary>("http://vocab.jncc.gov.uk/jncc-category");
            var counts = db.Query<JnccCategoriesIndex.Result, JnccCategoriesIndex>().ToList();

            var joined = from k in vocab.Keywords
                         from r in counts
                         where r.CategoryName == k.Value
                         orderby r.CategoryName != "Human Activities", r.CategoryName != "Seabed Habitat Maps"
                         select new CollectionOutputModel
                         {
                             Name = k.Value,
                             Description = k.Description,
                             RecordCount = r.RecordCount
                         };

            return joined.ToList();
        }
    }
}
