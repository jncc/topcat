using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Seed;
using Catalogue.Robot.DeadLinks;
using Catalogue.Web.Code;
using Raven.Client;

namespace Catalogue.Web.Controllers.Admin
{
    public class AdminController : ApiController
    {
        readonly IDocumentSession db;
        readonly IEnvironment environment;

        public AdminController(IDocumentSession db)
        {
            this.db = db;
        }

        [HttpGet, Route("api/admin/linkchecker")]
        public List<LinkCheckResult> LinkChecker()
        {
            var checker = new Checker(db, new FileLinkChecker());
            var results = checker.CheckAll();

            return results;
        }

        [HttpGet, Route("api/admin/seepath")]
        public bool SeePath()
        {
            return Directory.Exists(@"C:\topcat");
        }
    }
}