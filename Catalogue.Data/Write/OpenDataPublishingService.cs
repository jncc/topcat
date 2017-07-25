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
            if (record.Publication?.OpenData == null)
                throw new Exception("Couldn't sign-off record for publication. Assessment not completed.");

            var openDataInfo = record.Publication.OpenData;

            if (openDataInfo?.SignOff != null && openDataInfo.SignOff.DateUtc != DateTime.MinValue)
                throw new Exception("The record has already been signed off and cannot be signed off again.");

            if (openDataInfo?.Assessment != null && !openDataInfo.Assessment.Completed)
                throw new Exception("Couldn't sign-off record for publication. Assessment not completed.");

            openDataInfo.SignOff = signOffInfo;

            var recordServiceResult = recordService.Update(record);
            if (recordServiceResult.Success)
            {
                db.SaveChanges();
            }
            else
            {
                throw new Exception("Error while saving sign off changes.");
            }
        }
    }
}