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

namespace Catalogue.Robot.Publishing
{
    public class PublishingJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishingJob));

        private readonly Env env;
        private readonly IDocumentStore store;
        private readonly IDataUploader dataUploader;
        private readonly IHubService hubService;
        private readonly IGovService govService;

        public PublishingJob(Env env, IDocumentStore store, IDataUploader dataUploader, IHubService hubService, IGovService govService)
        {
            this.env = env;
            this.store = store;
            this.dataUploader = dataUploader;
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
                var publishingService = new RecordPublishingService(db, new RecordValidator(new VocabQueryer(db)));
                var publishingUploadService = publishingService.Upload();
                var redactor = new RecordRedactor(new VocabQueryer(db));

                var robotPublisher = new RobotPublisher(env, db, redactor, publishingUploadService, dataUploader, govService, hubService);

                recordsToPublish = robotPublisher.GetRecordsPendingUpload();
                Logger.Info($"Found {recordsToPublish.Count} records to publish");
            }

            
            foreach (var record in recordsToPublish)
            {
                using (var db = store.OpenSession())
                {
                    var publishingService = new RecordPublishingService(db, new RecordValidator(new VocabQueryer(db)));
                    var publishingUploadService = publishingService.Upload();
                    var redactor = new RecordRedactor(new VocabQueryer(db));

                    var robotPublisher = new RobotPublisher(env, db, redactor, publishingUploadService, dataUploader, govService, hubService);

                    robotPublisher.PublishRecord(record, metadataOnly);
                }
            }
            
        }
    }
}
