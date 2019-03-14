using Catalogue.Data.Query;
using Catalogue.Data.Write;
using Catalogue.Robot.Publishing.Data;
using Catalogue.Robot.Publishing.Gov;
using Catalogue.Robot.Publishing.Hub;
using log4net;
using Quartz;
using Raven.Client.Documents;

namespace Catalogue.Robot.Publishing
{
    public class PublishingJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishingJob));

        readonly IDocumentStore store;

        public PublishingJob(IDocumentStore store)
        {
            this.store = store;
        }

        public void Execute(IJobExecutionContext context)
        {
            Logger.Info("Executing job");
            Publish();
            Logger.Info("Finished all jobs");
        }

        public void Publish(bool metadataOnly = false)
        {
            using (var db = store.OpenSession())
            {
                var env = new Env();
                var publishingService = new RecordPublishingService(db, new RecordValidator());
                var publishingUploadService = publishingService.Upload();
                var redactor = new RecordRedactor(new VocabQueryer(db));
                var dataUploader = new DataUploader(env);
                var metadataUploader = new MetadataUploader(env);
                var hubService = new HubService(env);

                var robotPublisher = new RobotPublisher(env, db, redactor, publishingUploadService, dataUploader, metadataUploader, hubService);

                var records = robotPublisher.GetRecordsPendingUpload();
                Logger.Info($"Found {records.Count} records to upload");

                robotPublisher.PublishRecords(records, metadataOnly);
            }
        }
    }
}
