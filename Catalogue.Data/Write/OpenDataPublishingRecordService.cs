using Catalogue.Data.Model;
using System;
using Catalogue.Data.Query;
using Catalogue.Utilities.Time;
using Raven.Client;

namespace Catalogue.Data.Write
{
    public class OpenDataPublishingRecordService : RecordService, IOpenDataPublishingRecordService
    {
        public OpenDataPublishingRecordService(IDocumentSession db, IRecordValidator validator) : base(db, validator)
        {
        }

        public RecordOutputModel Assess(Record record, OpenDataAssessmentInfo assessmentInfo)
        {
            if (record.IsAssessedAndUpToDate())
                throw new InvalidOperationException("Assessment has already been completed and is up to date");

            if (!record.Validation.Equals(Validation.Gemini))
                throw new InvalidOperationException("Validation level must be Gemini");

            if (record.Publication == null)
            {
                record.Publication = new PublicationInfo();
            }

            if (record.Publication.OpenData == null)
            {
                record.Publication.OpenData = new OpenDataPublicationInfo
                {
                    Assessment = new OpenDataAssessmentInfo()
                };
            }

            record.Publication.OpenData.Assessment = assessmentInfo;
            UpdateMetadataDate(record, assessmentInfo.CompletedOnUtc);
            SetFooterForUpdatedRecord(record, assessmentInfo.CompletedByUser);

            var recordServiceResult = Upsert(record);
            if (!recordServiceResult.Success)
            {
                throw new Exception("Error while saving assessment changes.");
            }

            return recordServiceResult.RecordOutputModel;
        }

        public RecordOutputModel SignOff(Record record, OpenDataSignOffInfo signOffInfo)
        {
            if (!record.IsAssessedAndUpToDate())
                throw new InvalidOperationException("Couldn't sign-off record for publication - assessment not completed or out of date");

            if (record.IsSignedOffAndUpToDate())
                throw new InvalidOperationException("The record has already been signed off");

            record.Publication.OpenData.SignOff = signOffInfo;
            UpdateMetadataDate(record, signOffInfo.DateUtc);
            SetFooterForUpdatedRecord(record, signOffInfo.User);

            var recordServiceResult = Upsert(record);
            if (!recordServiceResult.Success)
                throw new Exception("Error while saving sign off changes");

            return recordServiceResult.RecordOutputModel;
        }

        public IOpenDataPublishingUploadRecordService Upload()
        {
            return new OpenDataPublishingUploadRecordService(db, validator);
        }

        private void UpdateMetadataDate(Record record, DateTime metadataDate)
        {
            record.Gemini.MetadataDate = metadataDate;
        }

        private void SetFooterForUpdatedRecord(Record record, UserInfo userInfo)
        {
            record.Footer.ModifiedOnUtc = Clock.NowUtc;
            record.Footer.ModifiedByUser = userInfo;
        }
    }
}