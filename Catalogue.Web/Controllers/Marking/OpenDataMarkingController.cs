using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Http;
using Catalogue.Data.Write;
using Raven.Client;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Controllers.Marking
{
    public class OpenDataMarkingController : ApiController
    {
        readonly IDocumentSession db;

        public OpenDataMarkingController(IDocumentSession db)
        {
            this.db = db;
        }

        [HttpGet, Route("api/marking/opendata")]
        public RecordServiceResult MarkAsOpenData(Guid id)
        {
            var recordServiceResult = new RecordServiceResult();
            var record = db.Load<Record>(id);

            if (record.Publication == null)
                record.Publication = new PublicationInfo();

            if (record.Publication.OpenData == null)
            {
                record.Publication.OpenData = new OpenDataPublicationInfo();
            }
                
            db.SaveChanges();

            return recordServiceResult;
        }
    }
}