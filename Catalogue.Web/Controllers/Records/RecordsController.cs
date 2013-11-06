using System;
using System.Web.Http;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.Records
{
    public class RecordsController : ApiController
    {
        readonly IDocumentSession db;

        public RecordsController(IDocumentSession db)
        {
            this.db = db;
        }

        // GET api/records/57d34691-9064-4c1e-90a7-7b0c112daa8d (get a record)
        public Record Get(Guid id)
        {
            return db.Load<Record>(id);
        }
    }
}

