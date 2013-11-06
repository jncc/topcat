using System;
using System.Web.Http;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Raven.Client;

namespace Catalogue.Web.Controllers.Records
{
    public class RecordsController : ApiController
    {
        readonly IDocumentSession db;
        readonly IRecordService service;

        public RecordsController(IRecordService service, IDocumentSession db)
        {
            this.service = service;
            this.db = db;
        }

        // GET api/records/57d34691-9064-4c1e-90a7-7b0c112daa8d (get a record)

        public Record Get(Guid id)
        {
            return service.Load(id);
        }

        // PUT api/records/57d34691-9064-4c1e-90a7-7b0c112daa8d (update/replace a record)

        public void Put(Guid id, [FromBody]Record record)
        {
            // todo should check ID in record is the same as being PUT to
            service.Insert(record);
            db.SaveChanges();
        }
    }
}

