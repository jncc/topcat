using System.Collections.Generic;
using Catalogue.Data.Model;

namespace Catalogue.Data.Write
{
    public interface IPublishingUploadRecordService
    {
        void UpdateDataPublishAttempt(Record record, PublicationAttempt attempt);
        void UpdateDataPublishSuccess(Record record, List<Resource> resources, PublicationAttempt attempt);
        void UpdateGovPublishAttempt(Record record, PublicationAttempt attempt);
        void UpdateGovPublishSuccess(Record record, PublicationAttempt attempt);
        void UpdateHubPublishAttempt(Record record, PublicationAttempt attempt);
        void UpdateHubPublishSuccess(Record record, string hubUrl, PublicationAttempt attempt);
    }
}
