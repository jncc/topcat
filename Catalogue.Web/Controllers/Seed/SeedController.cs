using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Data.Seed;

namespace Catalogue.Web.Controllers.Seed
{
    public class SeedController : ApiController
    {
        public void Seed()
        {
            // todo: erm, don't expose this
            Seeder.Seed(WebApiApplication.DocumentStore);
        }
    }
}
