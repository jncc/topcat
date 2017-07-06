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

        [HttpGet, Route("api/usage/recentlymodified")]
        public UsageOutputModel GetRecentlyModifiedRecords(DateTime dateTime)
        {
            var output = new UsageOutputModel();
            var recentlyModifiedRecords = db.Query<Record>().OrderByDescending(r => r.Gemini.MetadataDate).Take(5).ToList();

            output.RecentlyModifiedRecords = recentlyModifiedRecords;
            return output;
        }
    }
}