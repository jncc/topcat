using System;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Data.Write
{
    public class MarkingService : IMarkingService
    {
        private readonly IDocumentSession db;

        public MarkingService(IDocumentSession db)
        {
            this.db = db;
        }

        public void MarkAsOpenData(Guid id)
        {
            var record = db.Load<Record>(id);

            if (record.Publication == null)
                record.Publication = new PublicationInfo();

            if (record.Publication.OpenData == null)
            {
                record.Publication.OpenData = new OpenDataPublicationInfo();
            }

            db.SaveChanges();
        }
    }
}