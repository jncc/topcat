using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Seed;
using Raven.Client;

namespace Catalogue.Web.Controllers.Seed
{
    public class SeedController : ApiController
    {
        private IDocumentStore store;

        public SeedController(IDocumentStore store)
        {
            this.store = store;
        }

        /// <summary>
        /// Useful for non-live instances. Requires a POST so is difficult to do by accident.
        /// </summary>
        public void Post()
        {
            Seeder.Seed(store);
        }
    }
}