using System;
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
using System.Configuration;
using Catalogue.Data;
using Catalogue.Robot.Publishing.Client;
using Lucene.Net.Search;
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
        private readonly ISmtpClient smtpClient;

        public PublishingJob(Env env, IDocumentStore store, IDataService dataService, IHubService hubService, IGovService govService, ISmtpClient smtpClient)
        {
            this.env = env;
            this.store = store;
            this.dataService = dataService;
            this.hubService = hubService;
            this.govService = govService;
            this.smtpClient = smtpClient;
        }

        public void Execute(IJobExecutionContext context)
        {
            Logger.Info("Executing job");
            Publish();
            Logger.Info("Finished all jobs");
        }

        public void Publish(bool metadataOnly = false)
        {
            try
            {
                List<Record> recordsToPublish;
                
                // retrieve records
                using (var db = store.OpenSession())
                {
                    var robotPublisher = GetPublisher(db);

                    recordsToPublish = robotPublisher.GetRecordsPendingUpload();
                    Logger.Info($"Found {recordsToPublish.Count} records to publish");
                }

                var oldDataFiles = new Dictionary<string, List<string>>();

                // publish records
                foreach (var record in recordsToPublish)
                {
                    var recordId = Helpers.RemoveCollection(record.Id);
                    var dataFiles = dataService.GetDataFiles(recordId);

                    using (var db = store.OpenSession())
                    {
                        var robotPublisher = GetPublisher(db);
                        if (robotPublisher.PublishRecord(record, metadataOnly) && dataFiles.Count > 0)
                        {
                            oldDataFiles.Add(recordId, dataFiles);
                        }
                    }
                }

                // cleanup and reporting
                var removedFiles = new List<string>();
                foreach (var entry in oldDataFiles)
                {
                    var currentDataFiles = dataService.GetDataFiles(entry.Key);
                    foreach (var oldDataFile in entry.Value)
                    {
                        if (!currentDataFiles.Contains(oldDataFile))
                        {
                            removedFiles.Add(oldDataFile);
                        }
                    }

                    dataService.RemoveRollbackFiles(entry.Key);
                }

                dataService.ReportRemovedDataFiles(removedFiles);
            }
            catch (Exception e)
            {
                Logger.Error("Error in publishing process, attempting to send email alert", e);

                smtpClient.SendEmail(env.SMTP_FROM, env.SMTP_TO, "MEOW - Publishing error",
                    $"Something went wrong which caused the process to stop unexpectedly. Check the logs at {ConfigurationManager.AppSettings["LogFilePath"]}\n\n{e}");

                Logger.Info("Email sent successfully");
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
