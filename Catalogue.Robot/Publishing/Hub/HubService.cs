using Catalogue.Data.Model;
using System;
using log4net;

namespace Catalogue.Robot.Publishing.Hub
{
    public class HubService : IHubService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubService));

        public void Upsert(Record record)
        {
            // attempt saving to datahub db
            Logger.Info("Hub upsert operation not yet implemented");
        }

        public void Index(Record record)
        {
            // attempt indexing in elasticsearch
            Logger.Info("Hub index operation not yet implemented");
        }
    }
}
