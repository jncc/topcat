using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IRecordPublishingService
    {
        RecordServiceResult Assess(Record record, AssessmentInfo assessmentInfo);
        RecordServiceResult SignOff(Record record, SignOffInfo signOffInfo);
        IPublishingUploadRecordService Upload();
    }
}