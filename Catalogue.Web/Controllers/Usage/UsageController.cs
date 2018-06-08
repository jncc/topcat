﻿using Catalogue.Data.Model;
using System.Linq;
using System.Web.Http;
using Raven.Client.Documents.Session;

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

            var records = db.Query<Record>()
                .OrderByDescending(r => r.Footer.ModifiedOnUtc)
                .Take(5)
                .ToList();

            var recentlyModifiedRecords = records.Select(record => new RecentlyModifiedRecord
                {
                    Id = record.Id,
                    Title = record.Gemini.Title,
                    Date = record.Footer.ModifiedOnUtc,
                    User = record.Footer.ModifiedByUser.DisplayName,
                    Event = record.Footer.CreatedOnUtc.Equals(record.Footer.ModifiedOnUtc) ? "created" : "edited"
                })
                .ToList();

            output.RecentlyModifiedRecords = recentlyModifiedRecords;
            return output;
        }
    }
}