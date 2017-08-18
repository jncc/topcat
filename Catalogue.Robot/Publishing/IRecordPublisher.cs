using System;

namespace Catalogue.Robot.Publishing
{
    interface IRecordPublisher
    {
        void PublishRecord(Guid recordId);
    }
}
