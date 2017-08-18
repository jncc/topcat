using System.Collections.Generic;
using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IOpenDataPublishingService
    {
        Record Assess(Record record, OpenDataAssessmentInfo assessmentInfo);
        void SignOff(Record record, OpenDataSignOffInfo signOffInfo);
        void Upload(Record record, UserInfo userInfo, bool metadataOnly);
        List<Record> GetRecordsPendingUpload();
    }
}