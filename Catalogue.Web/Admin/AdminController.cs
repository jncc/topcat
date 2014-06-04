using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Catalogue.Data.Seed;

namespace Catalogue.Web.Admin
{
    public class AdminController : ApiController
    {
        //
        // GET: /Admin/
        public Boolean Get(string q)
        {

            Seeder.Seed();
        }

    

    }
}
