using Catalogue.Data.Model;
using Catalogue.Data.Query;

namespace Catalogue.Data.Write
{
    public interface IOpenDataPublishingRecordService
    {
        RecordOutputModel Assess(Record record, OpenDataAssessmentInfo assessmentInfo);
        RecordOutputModel SignOff(Record record, OpenDataSignOffInfo signOffInfo);
        IOpenDataPublishingUploadRecordService Upload();
    }
}