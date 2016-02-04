using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Web.Controllers.OpenDataPublishing
{
    public class OpenDataPublishingController : ApiController
    {
        readonly IDocumentSession db;

        public OpenDataPublishingController(IDocumentSession db)
        {
            this.db = db;
        }

        public OpenDataPublishingDisplayModel Get()
        {
            var recordsSuccessfullyPublishedCount = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.PublishedSinceLastUpdated)
                .Count();

            var recordsPendingPublicationCount = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => !x.PublishedSinceLastUpdated)
                .Count();

            return new OpenDataPublishingDisplayModel
                {
                    RecordsSuccessfullyPublishedCount = recordsSuccessfullyPublishedCount,
                    RecordsPendingPublicationCount = recordsPendingPublicationCount,
                };
        }
    }

    public class OpenDataPublishingDisplayModel
    {
        public int RecordsSuccessfullyPublishedCount { get; set; }
        public int RecordsPendingPublicationCount { get; set; }
    }
}


