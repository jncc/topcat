using System;

namespace Catalogue.Data.Write
{
    public interface IMarkingService
    {
        void MarkAsOpenData(Guid id);
    }
}