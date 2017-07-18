using System;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Data.Write
{
    public class PublishingService : IPublishingService
    {
        private readonly IDocumentSession db;
        private readonly IRecordService recordService;

        public PublishingService(IDocumentSession db, IRecordService recordService)
        {
            this.db = db;
            this.recordService = recordService;
        }

        public bool MarkForPublishing(Guid id)
        {
            var record = db.Load<Record>(id);

            if (record.Publication == null)
                record.Publication = new PublicationInfo();

            if (record.Publication.OpenData == null)
                record.Publication.OpenData = new OpenDataPublicationInfo();

            var recordServiceResult = recordService.Update(record);
            if (recordServiceResult.Success)
                db.SaveChanges();

            return recordServiceResult.Success;
        }

    }
}