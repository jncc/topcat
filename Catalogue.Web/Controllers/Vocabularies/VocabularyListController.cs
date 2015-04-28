using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.Vocabularies
{
    public class VocabularyListResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class VocabularyListController : ApiController
    {
        private readonly IDocumentSession _db; 

        public VocabularyListController(IDocumentSession db)
        {
            _db = db;
        }

        public ICollection<VocabularyListResult> Get()
        {
            return _db.Query<Vocabulary>().Select(v =>
                    new VocabularyListResult
                        {
                            Id = v.Id,
                            Name = v.Name,
                            Description = v.Description,
                        })
                        .ToList()
                        .OrderBy(v => v.Id == "http://vocab.jncc.gov.uk/jncc-domain")
                        .ThenBy(v => v.Id == "http://vocab.jncc.gov.uk/jncc-category")
                        .ThenBy(v => v.Name)
                        .ToList();
        }
    }
}