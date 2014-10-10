using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Raven.Client;

namespace Catalogue.Web.Controllers.Vocabularies
{
    public class VocabularyListController : ApiController
    {
        private readonly IDocumentSession _db; 

        public VocabularyListController(IDocumentSession db)
        {
            _db = db;
        }

        public ICollection<VocabularyIndex.Result> Get()
        {
            return _db.Query<VocabularyIndex.Result, VocabularyIndex>().Select(k => k).OrderBy(k => k.Vocab).ToList();
        }
    }
}