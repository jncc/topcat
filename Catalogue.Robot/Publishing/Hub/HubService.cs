using Catalogue.Data.Model;
using System;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubService : IHubService
    {
        public void Upsert(Record record)
        {
            // attempt saving to datahub db
            throw new NotImplementedException();
        }

        public void Index(Record record)
        {
            // attempt indexing in elasticsearch
            throw new NotImplementedException();
        }
    }
}
