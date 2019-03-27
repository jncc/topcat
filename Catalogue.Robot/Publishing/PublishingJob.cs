using System.Net.Http;
using Catalogue.Data.Query;
using Catalogue.Data.Write;
using Catalogue.Robot.Publishing.Client;
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

        private readonly Env env;
        private readonly IDocumentStore store;
        private readonly IFtpClient ftpClient;
        private readonly IApiClient apiClient;
        private readonly IQueueClient queueClient;

        public PublishingJob(Env env, IDocumentStore store, IFtpClient ftpClient, IApiClient apiClient, IQueueClient queueClient)
        {
            this.env = env;
            this.store = store;
            this.ftpClient = ftpClient;
            this.apiClient = apiClient;
            this.queueClient = queueClient;
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
                var publishingService = new RecordPublishingService(db, new RecordValidator());
                var publishingUploadService = publishingService.Upload();
                var redactor = new RecordRedactor(new VocabQueryer(db));
                var dataUploader = new DataUploader(env, ftpClient, new FileHelper());
                var govService = new GovService(env, ftpClient);
                var hubService = new HubService(env, apiClient, queueClient);

                var robotPublisher = new RobotPublisher(env, db, redactor, publishingUploadService, dataUploader, govService, hubService);

                var records = robotPublisher.GetRecordsPendingUpload();
                Logger.Info($"Found {records.Count} records to upload");

                robotPublisher.PublishRecords(records, metadataOnly);
            }
        }
    }
}
