using Catalogue.Data.Model;
using System;

namespace Catalogue.Data.Write
{
    public class OpenDataPublishingService : IOpenDataPublishingService
    {
        private readonly IRecordService recordService;

        public OpenDataPublishingService(IRecordService recordService)
        {
            this.recordService = recordService;
        }

        public Record Assess(Record record, OpenDataAssessmentInfo assessmentInfo)
        {
            if (!record.Validation.Equals(Validation.Gemini))
            {
                throw new Exception("Validation level must be Gemini.");
            }

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

            var assessment = record.Publication.OpenData.Assessment;
            if (assessment != null && assessment.Completed)
            {
                throw new Exception("Assessment has already been completed.");
            }

            record.Publication.OpenData.Assessment = assessmentInfo;

            var recordServiceResult = recordService.Update(record, assessmentInfo.CompletedByUser);
            if (!recordServiceResult.Success)
            {
                throw new Exception("Error while saving assessment changes.");
            }

            return recordServiceResult.Record;
        }

        public Record SignOff(Record record, OpenDataSignOffInfo signOffInfo)
        {
            if (!record.IsAssessedAndUpToDate())
                throw new InvalidOperationException("Couldn't sign-off record for publication - assessment not completed or out of date");

            if (record.IsSignedOffAndUpToDate())
                throw new InvalidOperationException("The record has already been signed off");

            record.Publication.OpenData.SignOff = signOffInfo;

            var recordServiceResult = recordService.Update(record, signOffInfo.User);
            if (!recordServiceResult.Success)
                throw new Exception("Error while saving sign off changes");

            return recordServiceResult.Record;
        }

        public IOpenDataPublishingUploadService Upload()
        {
            return new OpenDataPublishingUploadService(recordService);
        }
    }
}