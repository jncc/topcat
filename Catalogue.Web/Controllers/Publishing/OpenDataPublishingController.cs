using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.Publishing
{
    public class OpenDataPublishingController : ApiController
    {
        readonly IDocumentSession db;

        public OpenDataPublishingController(IDocumentSession db)
        {
            this.db = db;
        }

        [HttpGet, Route("api/publishing/opendata/summary")]
        public SummaryRepresentation Summary()
        {
            var query = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>();

            return new SummaryRepresentation
                {
                    CountOfPublishedSinceLastUpdated = query.Count(x => x.PublishedSinceLastUpdated),
                    CountOfNotYetPublishedSinceLastUpdated = query.Count(x => !x.PublishedSinceLastUpdated),
                    CountOfPublicationNeverAttempted = query.Count(x => x.PublicationNeverAttempted),
                    CountOfLastPublicationAttemptWasUnsuccessful = query.Count(x => x.LastPublicationAttemptWasUnsuccessful),
            };
        }

        [HttpGet, Route("api/publishing/opendata/pending")]
        public List<RecordRepresentation> Pending(int p = 1)
        {
            int take = 10;
            int skip = (p - 1) * take;

            var records = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => !x.PublishedSinceLastUpdated)
                .Skip(skip)
                .Take(take)
                .As<Record>()
                .ToList()
                .Select(r => new RecordRepresentation
                {
                    Id = r.Id,
                    Title = r.Gemini.Title,
                    MetadataDate = r.Gemini.MetadataDate,
                    IsGeminiValid = r.Validation == Validation.Gemini,
                    OpenData = r.Publication.OpenData,
                })
                .ToList();

            return records;
        }


    }

    public class RecordRepresentation
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime MetadataDate { get; set; }
        public bool IsGeminiValid { get; set; }
        public OpenDataPublicationInfo OpenData { get; set; }
    }

    public class SummaryRepresentation
    {
        public int CountOfPublishedSinceLastUpdated { get; set; }
        public int CountOfNotYetPublishedSinceLastUpdated { get; set; }
        public int CountOfPublicationNeverAttempted { get; set; }
        public int CountOfLastPublicationAttemptWasUnsuccessful { get; set; }
    }
}


