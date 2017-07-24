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
                throw new Exception("OpenDataAssessmentInfo not completed");

            var openDataInfo = record.Publication.OpenData;

            if (openDataInfo?.SignOff != null && openDataInfo.SignOff.DateUtc != DateTime.MinValue)
                throw new Exception("Record already signed off");

            if (openDataInfo?.Assessment != null && !openDataInfo.Assessment.Completed)
                throw new Exception("OpenDataAssessmentInfo not completed");

            openDataInfo.SignOff = signOffInfo;
            record.Publication.OpenData = openDataInfo;

            var recordServiceResult = recordService.Update(record);
            if (recordServiceResult.Success)
                db.SaveChanges();
        }
    }
}