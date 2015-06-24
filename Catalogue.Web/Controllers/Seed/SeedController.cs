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
using Raven.Abstractions.Commands;
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

        // todo delete when used on live
        [HttpPost, Route("api/seed/deletebadvocab")]
        public HttpResponseMessage DeleteBadVocab()
        {
            WebApiApplication.DocumentStore.DatabaseCommands.Batch(new[]
                {
                    new DeleteCommandData { Key = "http://vocab.jncc.gov.uk/metadata-administration" }
                });

            return new HttpResponseMessage { Content = new StringContent("Done") };
        }

    }
}