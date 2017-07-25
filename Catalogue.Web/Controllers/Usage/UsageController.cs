using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Raven.Client;
using Raven.Client.Linq;
using System;
using System.Linq;
using System.Web.Http;
using Catalogue.Data.Indexes;
using static Catalogue.Data.Model.RecordEvent;

namespace Catalogue.Web.Controllers.Usage
{
    public class UsageController : ApiController
    {
        readonly IDocumentSession db;

        public UsageController(IDocumentSession db)
        {
            this.db = db;
        }

        [HttpGet, Route("api/usage")]
        public UsageOutputModel GetRecentlyModifiedRecords()
        {
            var output = new UsageOutputModel();
            var recentlyModifiedRecords = db.Query<RecordEventIndex>()
                .As<RecentlyModifiedRecord>()
                .Take(5)
//                .Select(r => new RecentlyModifiedRecord
//                {
//                    Id = r.Id,
//                    Title = r.Gemini.Title,
//                    Date = r.Footer.ModifiedOnUtc, // ("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"),
//                    User = r.Footer.ModifiedBy,
//                    Event = r.Footer.CreatedOnUtc == r.Footer.ModifiedOnUtc ? Create : Edit
//                })
                .ToList();

            output.RecentlyModifiedRecords = recentlyModifiedRecords;
            return output;
        }
    }
}