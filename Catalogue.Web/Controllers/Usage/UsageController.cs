using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Raven.Client;
using Raven.Client.Linq;
using System;
using System.Linq;
using System.Web.Http;

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
            var recentlyModifiedRecords = db.Query<Record>()
                .OrderByDescending(r => r.Gemini.MetadataDate)
                .Take(5)
                .Select(r => new ModifiedRecord
                {
                    Id = r.Id,
                    Title = r.Gemini.Title,
                    Date = r.Gemini.MetadataDate, // ("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"),
                    User = r.Gemini.MetadataPointOfContact.Name
                })
                .ToList();

            output.RecentlyModifiedRecords = recentlyModifiedRecords;
            return output;
        }
    }
}