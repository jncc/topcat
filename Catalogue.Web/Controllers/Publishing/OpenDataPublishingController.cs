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
            var recordsSuccessfullyPublishedCount = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.PublishedSinceLastUpdated)
                .Count();

            var recordsPendingPublicationCount = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => !x.PublishedSinceLastUpdated)
                .Count();

            return new SummaryRepresentation
                {
                    RecordsSuccessfullyPublishedCount = recordsSuccessfullyPublishedCount,
                    RecordsPendingPublicationCount = recordsPendingPublicationCount,
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
        public OpenDataPublicationInfo OpenData { get; set; }
    }

    public class SummaryRepresentation
    {
        public int RecordsSuccessfullyPublishedCount { get; set; }
        public int RecordsPendingPublicationCount { get; set; }
    }
}


