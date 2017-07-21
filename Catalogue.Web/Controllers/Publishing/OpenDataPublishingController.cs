using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Web.Security;
using Raven.Client;

namespace Catalogue.Web.Controllers.Publishing
{
    public class OpenDataPublishingController : ApiController
    {
        readonly IDocumentSession db;
        readonly IOpenDataPublishingService openDataPublishingService;

        public OpenDataPublishingController(IDocumentSession db, IOpenDataPublishingService openDataPublishingService)
        {
            this.db = db;
            this.openDataPublishingService = openDataPublishingService;
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

        [HttpPut, Route("api/publishing/opendata/mark"), AuthorizeOpenDataPublishers]
        public IHttpActionResult MarkAsOpenData(Guid id)
        {
            if (openDataPublishingService.MarkForPublishing(id))
            {
                return Ok();
            }
            return InternalServerError();
        }

        [HttpGet, Route("api/publishing/opendata/publishedsincelastupdated")]
        public List<RecordRepresentation> PublishedSinceLastUpdated(int p = 1)
        {
            var query = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.PublishedSinceLastUpdated);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/opendata/notpublishedsincelastupdated")]
        public List<RecordRepresentation> Pending(int p = 1)
        {
            var query = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => !x.PublishedSinceLastUpdated);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/opendata/publicationneverattempted")]
        public List<RecordRepresentation> PublicationNeverAttempted(int p = 1)
        {
            var query = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.PublicationNeverAttempted);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/opendata/lastpublicationattemptwasunsuccessful")]
        public List<RecordRepresentation> LastPublicationAttemptWasUnsuccessful(int p = 1)
        {
            var query = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.LastPublicationAttemptWasUnsuccessful);

            return GetRecords(query, p);
        }

        List<RecordRepresentation> GetRecords(IQueryable<RecordsWithOpenDataPublicationInfoIndex.Result> query, int p)
        {
            int take = 1000; // can change this when paging is implemented in the UI
            int skip = (p - 1) * take;

            var records = query
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


