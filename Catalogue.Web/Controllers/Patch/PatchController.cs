using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Clone;
using Catalogue.Web.Injection;
using Raven.Client;

namespace Catalogue.Web.Controllers.Patch
{
    public class PatchController : ApiController
    {
        readonly IDocumentSession db;
        readonly IRecordQueryer queryer;

        public PatchController(IDocumentSession db, IRecordQueryer queryer)
        {
            this.db = db;
            this.queryer = queryer;
        }

        [HttpPost, Route("api/patch/fixupkeywords")]
        public HttpResponseMessage FixUpKeywords()
        {
            // mesh 
            var query1 = new RecordQueryInputModel
                {
                    K = new [] { "vocab.jncc.gov.uk/jncc-broad-category/Seabed Habitat Maps" },
                    P = 0,
                    N = 1024,
                };

            var records1 = queryer.RecordQuery(query1);

            foreach (var record in records1)
            {
                var existing = record.Gemini.Keywords.Single(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "Seabed Habitat Maps");
                record.Gemini.Keywords.Remove(existing);

                var domain = new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine" };
                var category = new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Seabed Habitat Maps" };

                record.Gemini.Keywords.Insert(0, domain);
                record.Gemini.Keywords.Insert(1, category);
            }

            var query2 = new RecordQueryInputModel
            {
                K = new[] { "vocab.jncc.gov.uk/jncc-broad-category/Marine Protected Areas" },
                P = 0,
                N = 1024,
            };

            var records2 = queryer.RecordQuery(query2);

            foreach (var record in records2)
            {
                var existing = record.Gemini.Keywords.Single(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "Marine Protected Sites");
                record.Gemini.Keywords.Remove(existing);

                var domain = new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine" };
                var category = new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Protected Areas" };

                record.Gemini.Keywords.Insert(0, domain);
                record.Gemini.Keywords.Insert(1, category);
            }


            db.SaveChanges();

            return new HttpResponseMessage();
        }

    }
}
