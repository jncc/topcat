using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Seed;
using Catalogue.Web.Code;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide.Operations;

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

        [HttpPost, Route("api/admin/renamekeyword")]
        public string RenameKeyword(string keyword, string newValue)
        {
            var query = new RecordQueryInputModel
            {
                F = new FilterOptions{Keywords = new[] { keyword }},
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

    }
}