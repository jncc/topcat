﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using Catalogue.Data.Indexes;
using System.Linq;
using Catalogue.Web.Admin.Keywords;
using Raven.Client;

namespace Catalogue.Web.Controllers.Search
{
    public class VocabularyTypeaheadController : ApiController
    {
        private readonly IDocumentSession _db;

        public VocabularyTypeaheadController(IDocumentSession db)
        {
            _db = db;
        }

        public ICollection<string> Get(String q)
        {
            if (String.IsNullOrWhiteSpace(q)) return new List<string>();

            var containsTerm = "*" + q.Trim().Replace("*", String.Empty) + "*";

            return _db.Query<VocabularyIndex.Result, VocabularyIndex>()
                .Search(k => k.Vocab, containsTerm, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards)
                .Select(k => k.Vocab)
                .ToList();
        }
    }
}