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
            
            var counts = db.Query<RecordCountForKeywordIndex.Result, RecordCountForKeywordIndex>()
                .Where(r => r.KeywordVocab == "http://vocab.jncc.gov.uk/jncc-category")
                .ToList();

            var joined = from k in vocab.Keywords
                         from r in counts
                         where r.KeywordValue == k.Value
                         orderby r.KeywordValue != "Human Activities",
                            r.KeywordValue != "Seabed Habitat Maps"
                         select new CollectionOutputModel
                         {
                             Name = k.Value,
                             Description = k.Description,
                             RecordCount = r.RecordCount
                         };

            var output = joined.ToList();

            // add some future collections with records yet to be added to...
            output.AddRange(from k in vocab.Keywords
                            where k.Value == "Natural Capital Library" || k.Value == "JNCC Publications"
                            where !counts.Any(c => c.KeywordValue == k.Value) // no records for these collections
                            select new CollectionOutputModel
                            {
                                Name = k.Value,
                                Description = k.Description,
                                RecordCount = 0,
                            });

            return output;
        }
    }
}
