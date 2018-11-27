﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Gemini.Model;
using Raven.Client.Documents.Session;

namespace Catalogue.Web.Controllers.Vocabularies
{
    public class VocabularyListResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Controlled { get; set; }
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
                            Controlled = v.Controlled,
                        })
                        .ToList()
                        .OrderByDescending(v => v.Id == "http://vocab.jncc.gov.uk/jncc-domain")
                        .ThenByDescending(v => v.Id == "http://vocab.jncc.gov.uk/jncc-category")
                        .ThenBy(v => v.Name)
                        .ToList();
        }
    }
}