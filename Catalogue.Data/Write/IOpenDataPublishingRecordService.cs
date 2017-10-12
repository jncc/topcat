using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IOpenDataPublishingRecordService
    {
        RecordServiceResult Assess(Record record, OpenDataAssessmentInfo assessmentInfo);
        RecordServiceResult SignOff(Record record, OpenDataSignOffInfo signOffInfo);
        IOpenDataPublishingUploadRecordService Upload();
    }
}