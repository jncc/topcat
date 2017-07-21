using System;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Data.Write
{
    public class OpenDataPublishingService : IOpenDataPublishingService
    {
        private readonly IDocumentSession db;
        private readonly IRecordService recordService;

        public OpenDataPublishingService(IDocumentSession db, IRecordService recordService)
        {
            this.db = db;
            this.recordService = recordService;
        }

        public void SignOff(Record record, OpenDataSignOffInfo signOffInfo)
        {
            var publicationInfo = record.Publication;

            if (publicationInfo == null)
                record.Publication = new PublicationInfo();

            var openDataInfo = record.Publication.OpenData ?? new OpenDataPublicationInfo();
            openDataInfo.SignOff = signOffInfo;
            record.Publication.OpenData = openDataInfo;

            var recordServiceResult = recordService.Update(record);
            if (recordServiceResult.Success)
                db.SaveChanges();
        }

    }
}