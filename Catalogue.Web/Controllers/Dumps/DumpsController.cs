﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.Dumps
{
    public class DumpsController : ApiController
    {
        readonly IDocumentSession db;

        public DumpsController(IDocumentSession db)
        {
            this.db = db;
        }

        [HttpGet, Route("api/dumps/allkeywordsinrecords")]
        public List<MetadataKeyword> AllKeywordsInRecords()
        {
            var q = db.Query<RecordKeywordIndex.Result, RecordKeywordIndex>();

            var result = q.Take(1024).ToList();
            if (result.Count == 1024) throw new Exception("Too many results. Needs to page them!");
            
            return result.Select(k => new MetadataKeyword { Vocab = k.Vocab, Value = k.Value }).ToList();
        }
    }
}
