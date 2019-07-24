using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Write;
using Catalogue.Robot.Publishing.Data;
using Catalogue.Robot.Publishing.Gov;
using Catalogue.Robot.Publishing.Hub;
using log4net;
using Quartz;
using Raven.Client.Documents;
using System.Collections.Generic;
using Raven.Client.Documents.Session;

namespace Catalogue.Robot.Publishing
{
    public class PublishingJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishingJob));

        private readonly Env env;
        private readonly IDocumentStore store;
        private readonly IDataService dataService;
        private readonly IHubService hubService;
        private readonly IGovService govService;

        public PublishingJob(Env env, IDocumentStore store, IDataService dataService, IHubService hubService, IGovService govService)
        {
            this.env = env;
            this.store = store;
            this.dataService = dataService;
            this.hubService = hubService;
            this.govService = govService;
        }

        public void Execute(IJobExecutionContext context)
        {
            Logger.Info("Executing job");
            Publish();
            Logger.Info("Finished all jobs");
        }

        public void Publish(bool metadataOnly = false)
        {
            List<Record> recordsToPublish;

            using (var db = store.OpenSession())
            {
                var robotPublisher = GetPublisher(db);

                recordsToPublish = robotPublisher.GetRecordsPendingUpload();
                Logger.Info($"Found {recordsToPublish.Count} records to publish");
            }

            
            foreach (var record in recordsToPublish)
            {
                using (var db = store.OpenSession())
                {
                    var robotPublisher = GetPublisher(db);
                    robotPublisher.PublishRecord(record, metadataOnly);
                }
            }
            
        }

        private RobotPublisher GetPublisher(IDocumentSession db)
        {
            var publishingService = new RecordPublishingService(db, new RecordValidator(new VocabQueryer(db)));
            var publishingUploadService = publishingService.Upload();
            var redactor = new RecordRedactor(new VocabQueryer(db));

            return new RobotPublisher(env, db, redactor, publishingUploadService, dataService, govService, hubService);
        }
    }
}
