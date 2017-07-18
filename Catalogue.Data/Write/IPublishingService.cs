using System;

namespace Catalogue.Data.Write
{
    public interface IPublishingService
    {
        bool MarkForPublishing(Guid id);
        // TODO
        // SignOff
        // Publish
    }
}