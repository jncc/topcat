using Catalogue.Data;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Utilities.Time;
using log4net;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Catalogue.Data.Extensions;
using Catalogue.Robot.Publishing.Data;
using Catalogue.Robot.Publishing.Gov;
using Catalogue.Utilities.Clone;
using static Catalogue.Utilities.Text.WebificationUtility;

namespace Catalogue.Robot.Publishing
{
    public class RobotPublisher
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RobotPublisher));

        private readonly Env env;
        private readonly IDocumentSession db;
        private readonly IRecordRedactor recordRedactor;
        private readonly IPublishingUploadRecordService uploadRecordService;
        private readonly IDataUploader dataUploader;
        private readonly IMetadataUploader metadataUploader;
        private readonly IHubService hubService;

        public RobotPublisher(
            Env env,
            IDocumentSession db,
            IRecordRedactor recordRedactor,
            IPublishingUploadRecordService uploadRecordService,
            IDataUploader dataUploader,
            IMetadataUploader metadataUploader,
            IHubService hubService
            )
        {
            this.env = env;
            this.db = db;
            this.recordRedactor = recordRedactor;
            this.uploadRecordService = uploadRecordService;
            this.dataUploader = dataUploader;
            this.metadataUploader = metadataUploader;
            this.hubService = hubService;
        }

        public List<Record> GetRecordsPendingUpload()
        {
            var records = db.Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Customize(x => x.WaitForNonStaleResults())
                .Where(x => x.Assessed)
                .Where(x => x.SignedOff)
                .Where(x => x.GeminiValidated) // all open data should be gemini-valid - this is a safety
                .Where(x => x.HubPublishable && !x.PublishedToHubSinceLastUpdated || x.GovPublishable && !x.PublishedToGovSinceLastUpdated)
                .OfType<Record>() //.Select(r => r.Id) // this doesn't work in RavenDB, and doesn't throw!
                .Take(1000) // so take 1000 which is enough for one run
                .ToList();

            return records;
        }

        public void PublishRecords(List<Record> records, bool metadataOnly = false)
        {
            foreach (Record record in records)
            {
                Logger.Info($"Starting publishing process for record {record.Id} {record.Gemini.Title}");
                if (!record.HasPublishingTarget())
                {
                    Logger.Info("No publishing targets defined, aborting publishing");
                    return;
                }
                
                try
                {
                    if (!metadataOnly)
                    {
                        PublishDataFiles(record);
                    }
                    
                    PublishHubMetadata(record);
                    PublishGovMetadata(record);
                }
                catch (WebException ex)
                {
                    Logger.Error($"Could not complete publishing process for record with GUID={record.Id}", ex);
                }

                Logger.Info($"Successfully published record {record.Id} {record.Gemini.Title}");
            }
            // commit the changes - to both the record (resource locator may have changed) and the attempt object
            db.SaveChanges();
        }

        private void PublishDataFiles(Record record)
        {
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            uploadRecordService.UpdateDataPublishAttempt(record, attempt);
            db.SaveChanges();

            try
            {
                var resources = record.Publication.Data.Resources.Copy(); // don't want to save if any of them fail
                if (resources != null)
                {
                    Logger.Info($"Found {record.Publication.Data.Resources.Count} publishable resources");
                    foreach (var resource in resources)
                    {
                        if (Helpers.IsFileResource(resource))
                        {
                            Logger.Info($"Resource {resource.Path} is a file - starting upload process");
                            dataUploader.UploadDataFile(Helpers.RemoveCollection(record.Id), resource.Path);
                            string dataHttpPath = env.HTTP_ROOT_URL + "/" +
                                                  GetUnrootedDataPath(Helpers.RemoveCollection(record.Id),
                                                      resource.Path);
                            resource.PublishedUrl = dataHttpPath;
                        }
                        else
                        {
                            Logger.Info($"Resource {resource.Path} is a URL - no file to upload");

                            resource.PublishedUrl = null; // make sure this is "clean" if no data file was uploaded
                        }
                    }
                }

                uploadRecordService.UpdateDataPublishSuccess(record, resources, attempt);
            }
            catch (WebException ex)
            {
                attempt.Message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logger.Error($"Data transfer failed for record with GUID={record.Id}", ex);
                throw;
            }
        }

        public void PublishHubMetadata(Record record)
        {
            if (record.Publication.Target?.Hub?.Publishable == true)
            {
                var attempt = new PublicationAttempt {DateUtc = Clock.NowUtc};
                uploadRecordService.UpdateHubPublishAttempt(record, attempt);
                db.SaveChanges();

                try
                {
                    var redactedRecord = recordRedactor.RedactRecord(record); // this isn't saved back to the db

                    hubService.Save(redactedRecord);

                    var url = env.HUB_ASSETS_BASE_URL + Helpers.RemoveCollection(record.Id);
                    uploadRecordService.UpdateHubPublishSuccess(record, url, attempt);

                    hubService.Index(redactedRecord);
                }
                catch (WebException ex)
                {
                    attempt.Message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                    Logger.Error($"Could not save record to hub database, GUID={record.Id}", ex);
                    throw;
                }
            }
            else
            {
                Logger.Info("Hub not defined as a target publishing destination");
            }
        }

        public void PublishGovMetadata(Record record)
        {
            if (record.Publication.Target?.Gov?.Publishable == true) {
                var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
                uploadRecordService.UpdateGovPublishAttempt(record, attempt);
                db.SaveChanges();

                try
                {
                    var redactedRecord = recordRedactor.RedactRecord(record); // this isn't saved back to the db
                    metadataUploader.UploadMetadataDocument(Helpers.RemoveCollectionFromId(redactedRecord));
                    metadataUploader.UploadWafIndexDocument(Helpers.RemoveCollectionFromId(redactedRecord));

                    uploadRecordService.UpdateGovPublishSuccess(record, attempt);
                }
                catch (WebException ex)
                {
                    attempt.Message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                    Logger.Error($"DGU metadata transfer failed for record with GUID={record.Id}", ex);
                    throw;
                }
            }
            else
            {
                Logger.Info("Gov not defined as a target publishing destination");
            }
        }
    }
}
