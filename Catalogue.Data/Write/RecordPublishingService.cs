using Catalogue.Data.Model;
using Catalogue.Utilities.Time;
using System;
using Catalogue.Data.Extensions;
using Raven.Client.Documents.Session;
using static Catalogue.Data.Write.RecordServiceHelper;

namespace Catalogue.Data.Write
{
    public class RecordPublishingService : IRecordPublishingService
    {
        private readonly IDocumentSession db;
        private readonly IRecordValidator validator;

        public RecordPublishingService(IDocumentSession db, IRecordValidator validator)
        {
            this.db = db;
            this.validator = validator;
        }

        public RecordServiceResult Assess(Record record, AssessmentInfo assessmentInfo)
        {
            if (record.IsAssessedAndUpToDate())
                throw new InvalidOperationException("Assessment has already been completed and is up to date");

            if (!record.Validation.Equals(Validation.Gemini))
                throw new InvalidOperationException("Validation level must be Gemini");

            if (!record.HasPublishingDestination())
            {
                throw new InvalidOperationException("Must select at least one publishing destination");
            }

            record.Publication.Assessment = assessmentInfo;
            UpdateMetadataDate(record, assessmentInfo.CompletedOnUtc);
            SetFooterForUpdatedRecord(record, assessmentInfo.CompletedByUser);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                throw new Exception("Error while saving assessment changes.");
            }

            return recordServiceResult;
        }

        public RecordServiceResult SignOff(Record record, SignOffInfo signOffInfo)
        {
            if (!record.IsAssessedAndUpToDate())
                throw new InvalidOperationException("Couldn't sign-off record for publication - assessment not completed or out of date");

            if (record.IsSignedOffAndUpToDate())
                throw new InvalidOperationException("The record has already been signed off");

            record.Publication.SignOff = signOffInfo;
            UpdateMetadataDate(record, signOffInfo.DateUtc);
            SetFooterForUpdatedRecord(record, signOffInfo.User);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
                throw new Exception("Error while saving sign off changes");

            return recordServiceResult;
        }

        public IPublishingUploadRecordService Upload()
        {
            return new PublishingUploadRecordService(db, validator);
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