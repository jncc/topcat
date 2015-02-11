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
            return (from v in _db.Query<Vocabulary>()
                    orderby v.Name
                    select new VocabularyListResult()
                        {
                            Id = v.Id,
                            Name = v.Name
                        }).ToList();
                   
        }
    }
}