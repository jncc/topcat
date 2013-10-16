using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Data.Seed;

namespace Catalogue.Web.Controllers.Seed
{
    public class SeedController : ApiController
    {
        public string Get()
        {
            //if (ConfigurationManager.AppSettings["Environment"] == "Dev")

            // todo: erm, don't expose this
            Seeder.Seed(WebApiApplication.DocumentStore);

            return "Seeded";
        }
    }
}
