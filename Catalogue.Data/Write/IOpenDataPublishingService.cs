using System.Collections.Generic;
using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IOpenDataPublishingService
    {
        Record Assess(Record record, OpenDataAssessmentInfo assessmentInfo);
        Record SignOff(Record record, OpenDataSignOffInfo signOffInfo);
        IOpenDataPublishingUploadService Upload(Record record, UserInfo userInfo, bool metadataOnly);
    }
}