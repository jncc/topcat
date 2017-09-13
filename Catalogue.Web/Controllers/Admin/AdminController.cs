using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Seed;
using Catalogue.Web.Code;
using Raven.Abstractions.Data;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace Catalogue.Web.Controllers.Admin
{
    /// <summary>
    /// This controller is not intended to have any UI. Post the HTTP requests using Postman or something.
    /// </summary>
    public class AdminController : ApiController
    {
        readonly IDocumentSession db;
        readonly IEnvironment environment;
        readonly IRecordQueryer recordQueryer;

        public AdminController(IDocumentSession db, IEnvironment environment, IRecordQueryer recordQueryer)
        {
            this.db = db;
            this.environment = environment;
            this.recordQueryer = recordQueryer;
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
        /// Deletes all the records tagged with the special 'Delete' tag. This has been added to the Robot so could be deleted.
        /// </summary>
        [HttpPost, Route("api/admin/delete")]
        public HttpResponseMessage Delete()
        {

            string luceneQuery = "Keywords:\"http://vocab.jncc.gov.uk/metadata-admin/Delete\"";
            int recordsToDelete = db.Advanced.DocumentQuery<Record>("RecordIndex").Where(luceneQuery).ToList().Count;

            WebApiApplication.DocumentStore.DatabaseCommands.DeleteByIndex("RecordIndex",
                new IndexQuery
                {
                    Query = "Keywords:\"http://vocab.jncc.gov.uk/metadata-admin/Delete\""
                });

            return new HttpResponseMessage { Content = new StringContent("Asked to delete "  + recordsToDelete + " records") };
        }

        /// <summary>
        /// Deletes all the records in the given category.
        /// May need to use the querystring, e.g http://topcat-beta/api/admin/deletejncccategory?category=Marine+Recorder
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

        [HttpPost, Route("api/admin/renamekeyword")]
        public string RenameKeyword(string keyword, string newValue)
        {
            var query = new RecordQueryInputModel
            {
                K = new[] { keyword },
                N = 1024,
            };

            int count = recordQueryer.Query(query).Count();

            var records = recordQueryer.Query(query).ToList();

            if (records.Count != count)
                throw new Exception("Too many records.");

            foreach (var record in records)
            {
                var kword = ParameterHelper.ParseMetadataKeywords(new [] { keyword }).Single();
                var keywordToChange = record.Gemini.Keywords.Single(k => k.Vocab == kword.Vocab && k.Value == kword.Value);
                keywordToChange.Value = newValue;
            }

            db.SaveChanges();

            return String.Format("{0} records updated.", count);
        }

        [HttpGet, Route("api/admin/temptest")]
        public bool TempTest()
        {
            return true;
        }
		
        [HttpGet, Route("api/admin/seepath")]
        public bool SeePath()
        {
            return Directory.Exists(@"C:\topcat");
        }

    }
}