using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Seed;
using Catalogue.Gemini.Model;
using Catalogue.Web.Code;
using Raven.Abstractions.Data;
using Raven.Client;

namespace Catalogue.Web.Controllers.Seed
{
    public class SeedController : ApiController
    {
        readonly IEnvironment environment;
        readonly IDocumentSession db;

        public SeedController(IEnvironment environment, IDocumentSession db)
        {
            this.environment = environment;
            this.db = db;
        }

        /// <summary>
        /// Useful for non-live instances. Requires a POST so is difficult to do by accident.
        /// </summary>
        [HttpPost, Route("api/seed/all")]
        public HttpResponseMessage All()
        {
            if (environment.Name == "Live")
                throw new InvalidOperationException("Oops, you surely didn't mean to seed the live instance..?");

            Seeder.Seed(WebApiApplication.DocumentStore);

            return new HttpResponseMessage { Content = new StringContent("Done") };
        }

        [HttpPost, Route("api/seed/wipedb")]
        public HttpResponseMessage WipeDB()
        {
            if (environment.Name == "Live")
                throw new InvalidOperationException("Oops, you definitely didn't mean to delete the live instance.");

            WebApiApplication.DocumentStore.DatabaseCommands.DeleteByIndex("Raven/DocumentsByEntityName",
                new IndexQuery());

            return new HttpResponseMessage { Content = new StringContent("Done") };
        }

        [HttpPost, Route("api/seed/deletemesh")]
        public HttpResponseMessage DeleteMesh()
        {
            if (environment.Name == "Live")
                throw new InvalidOperationException("Oops, you surely didn't mean to seed the live instance..?");

            WebApiApplication.DocumentStore.DatabaseCommands.DeleteByIndex("RecordIndex",
                new IndexQuery
                {
                    Query = "Keywords:\"http://vocab.jncc.gov.uk/jncc-category/Seabed Habitat Maps\""
                });

            return new HttpResponseMessage { Content = new StringContent("Done") };
        }

        // todo delete when created on live
        [HttpPost, Route("api/seed/createmetadataadmin")]
        public HttpResponseMessage MetadataAdmin()
        {
            var metadataAdmin = new Vocabulary
            {
                Id = "http://vocab.jncc.gov.uk/metadata-admin",
                Name = "Metadata Admin",
                Description = "Tags for managing Topcat records.",
                PublicationDate = "2015",
                Publishable = false,
                Controlled = true,
                Keywords = new List<VocabularyKeyword>
                        {
                            new VocabularyKeyword { Value = "Delete" },
                            new VocabularyKeyword { Value = "Improve" },
                            new VocabularyKeyword { Value = "Suspect" },
                        }
            };
            db.Store(metadataAdmin);
            db.SaveChanges();

            return new HttpResponseMessage { Content = new StringContent("Done") };
        }

    }
}