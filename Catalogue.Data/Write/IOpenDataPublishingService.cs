using System;

namespace Catalogue.Data.Write
{
    public interface IOpenDataPublishingService
    {
        bool MarkForPublishing(Guid id);
        // TODO
        // SignOff
        // Publish
    }
}