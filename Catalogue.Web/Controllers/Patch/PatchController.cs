using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Data.Query;
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
//            // mesh 
//            var query1 = new RecordQueryInputModel
//                {
//                    K = new [] { "vocab.jncc.gov.uk/jncc-broad-category/Seabed Habitat Maps" },
//                    P = 0,
//                    N = 1024,
//                };
//
//            var records1 = queryer.RecordQuery(query1);
//
//            foreach (var record in records1)
//            {
//                var existing = record.Gemini.Keywords.Single(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category" && k.Value == "Seabed Habitat Maps");
//                record.Gemini.Keywords.Remove(existing);
//
//                var domain = new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-domain", Value = "Marine" };
//                var category = new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/jncc-category", Value = "Seabed Habitat Maps" };
//
//                record.Gemini.Keywords.Insert(0, domain);
//                record.Gemini.Keywords.Insert(1, category);
//            }

            // protected sites
            var query2 = new RecordQueryInputModel
            {
                K = new[] { "vocab.jncc.gov.uk/jncc-broad-category/Marine Protected Sites" },
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

        [HttpPost, Route("api/patch/fixupvocabs")]
        public HttpResponseMessage FixUpVocabs()
        {
            var broad = db.Load<Vocabulary>("http://vocab.jncc.gov.uk/jncc-broad-category");

            db.Delete(broad);

            var domain = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/jncc-domain",
                Name = "JNCC Domain",
                Description = "The broad domain within JNCC.",
                PublicationDate = "2015",
                Publishable = true,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "Marine" },
                            new VocabularyKeyword { Value = "Freshwater" },
                            new VocabularyKeyword { Value = "Terrestrial" },
                            new VocabularyKeyword { Value = "Atmosphere" },
                        }
            };
            db.Store(domain);

            var category = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/jncc-category",
                Name = "JNCC Category",
                Description = "The data category within JNCC.",
                PublicationDate = "2015",
                Publishable = true,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "Seabed Habitat Maps" },
                            new VocabularyKeyword { Value = "Protected Areas" },
                        }
            };
            db.Store(category);


            db.SaveChanges();
            return new HttpResponseMessage();
        }

        [HttpPost, Route("api/patch/fixupactivitiesemailaddress")]
        public HttpResponseMessage FixUpActivitiesEmailAddress()
        {
            var query = new RecordQueryInputModel
            {
                K = new[] { "vocab.jncc.gov.uk/jncc-category/Human Activities" },
                P = 0,
                N = 1024,
            };

            var records = queryer.RecordQuery(query);

            foreach (var record in records)
            {
                record.Gemini.MetadataPointOfContact.Email = record.Gemini.MetadataPointOfContact.Email.Replace("jnccc", "jncc");
            }

            db.SaveChanges();

            return new HttpResponseMessage();
        }
    }
}
