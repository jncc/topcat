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
using Raven.Abstractions.Data;
using Raven.Client;

namespace Catalogue.Web.Controllers.Admin
{
    /// <summary>
    /// This controller is not intended to have any UI. Post the HTTP requests using Postman or something.
    /// </summary>
    public class AdminController : ApiController
    {
        readonly IDocumentSession db;
        readonly IEnvironment environment;

        public AdminController(IDocumentSession db, IEnvironment environment)
        {
            this.db = db;
            this.environment = environment;
        }

        void ThrowIfLiveEnvironment()
        {
            if (environment.Name == "Live")
                throw new InvalidOperationException("You didn't mean to do this to the live instance.");
        }

        /// <summary>
        /// Seeds the database with the seed data. Useful for non-live instances.
        /// </summary>
        [HttpPost, Route("api/admin/seed")]
        public HttpResponseMessage Seed()
        {
            ThrowIfLiveEnvironment();

            Seeder.Seed(WebApiApplication.DocumentStore);

            return new HttpResponseMessage { Content = new StringContent("Done") };
        }

        /// <summary>
        /// Wipes the entire database. Got that?
        /// </summary>
        [HttpPost, Route("api/admin/destroy")]
        public HttpResponseMessage Destroy()
        {
            ThrowIfLiveEnvironment();

            WebApiApplication.DocumentStore.DatabaseCommands.DeleteByIndex("Raven/DocumentsByEntityName", new IndexQuery());

            return new HttpResponseMessage { Content = new StringContent("Done") };
        }

        /// <summary>
        /// Deletes all the records tagged with the special 'Delete' tag.
        /// </summary>
        [HttpPost, Route("api/admin/delete")]
        public HttpResponseMessage Delete()
        {
            WebApiApplication.DocumentStore.DatabaseCommands.DeleteByIndex("RecordIndex",
                new IndexQuery
                {
                    Query = "Keywords:\"http://vocab.jncc.gov.uk/metadata-admin/Delete\""
                });

            return new HttpResponseMessage { Content = new StringContent("Done") };
        }

        /// <summary>
        /// Deletes all the records in the given category.
        /// </summary>
        [HttpPost, Route("api/admin/deletejncccategory")]
        public HttpResponseMessage DeleteJnccCategory(string category)
        {
            ThrowIfLiveEnvironment();

            WebApiApplication.DocumentStore.DatabaseCommands.DeleteByIndex("RecordIndex",
                new IndexQuery
                {
                    Query = String.Format("Keywords:\"http://vocab.jncc.gov.uk/jncc-category/{0}\"", category)
                });

            return new HttpResponseMessage { Content = new StringContent("Done") };
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