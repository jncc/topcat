using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Seed;
using Catalogue.Web.Code;
using Raven.Client;

namespace Catalogue.Web.Controllers.Seed
{
    public class SeedController : ApiController
    {
        readonly IEnvironment environment;

        public SeedController(IEnvironment environment)
        {
            this.environment = environment;
        }

        /// <summary>
        /// Useful for non-live instances. Requires a POST so is difficult to do by accident.
        /// </summary>
        public HttpResponseMessage Post()
        {
            if (environment.Name == "Live")
                throw new InvalidOperationException("Oops, you surely didn't mean to seed the live instance..?");

            Seeder.Seed(WebApiApplication.DocumentStore);

            return new HttpResponseMessage();
        }

        /// <summary>
        /// Todo remove this
        /// Useful for non-live instances. Requires a PUT so is difficult to do by accident.
        /// </summary>
        public HttpResponseMessage Put()
        {
            Seeder.SeedVocabsOnly(WebApiApplication.DocumentStore);

            return new HttpResponseMessage();
        }
    }
}