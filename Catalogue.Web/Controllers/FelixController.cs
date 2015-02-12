using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers
{
    public class FelixController : ApiController
    {
        private readonly IDocumentSession db;

        public FelixController(IDocumentSession db)
        {
            this.db = db;
        }

        public Vocabulary Get()
        {
            var vocabulary = db.Load<Vocabulary>("http://vocab.jncc.gov.uk/jncc-broad-category");
            return vocabulary;
        }

    }
}
