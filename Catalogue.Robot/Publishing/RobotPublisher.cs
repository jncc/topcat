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
        private readonly IGovService govService;
        private readonly IHubService hubService;

        public RobotPublisher(
            Env env,
            IDocumentSession db,
            IRecordRedactor recordRedactor,
            IPublishingUploadRecordService uploadRecordService,
            IDataUploader dataUploader,
            IGovService govService,
            IHubService hubService
            )
        {
            this.env = env;
            this.db = db;
            this.recordRedactor = recordRedactor;
            this.uploadRecordService = uploadRecordService;
            this.dataUploader = dataUploader;
            this.govService = govService;
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

                try
                {
                    if (!metadataOnly)
                    {
                        PublishDataFiles(record);
                    }

                    PublishHubMetadata(record);
                    PublishGovMetadata(record);

                    Logger.Info($"Successfully published record {record.Id} {record.Gemini.Title}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Could not complete publishing process for record with GUID={record.Id}", ex);
                }
                finally
                {
                    // commit the changes - to both the record (resource locator may have changed) and the attempt object
                    db.SaveChanges();
                }
            }
        }

        private void PublishDataFiles(Record record)
        {
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            try
            {
                uploadRecordService.UpdateDataPublishAttempt(record, attempt);
                db.SaveChanges();

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
                            string dataHttpPath = env.HTTP_ROOT_URL + GetUnrootedDataPath(Helpers.RemoveCollection(record.Id), resource.Path);
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
            catch (Exception ex)
            {
                attempt.Message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logger.Error($"Data transfer failed for record with GUID={record.Id}", ex);
                throw;
            }

            //todo: save changes here in finally
        }

        public void PublishHubMetadata(Record record)
        {
            if (record.Publication.Target?.Hub?.Publishable == true)
            {
                var attempt = new PublicationAttempt {DateUtc = Clock.NowUtc};
                Record redactedRecord;

                try
                {
                    uploadRecordService.UpdateHubPublishAttempt(record, attempt);
                    db.SaveChanges();

                    redactedRecord = recordRedactor.RedactRecord(record); // this isn't saved back to the db

                    hubService.Publish(redactedRecord);

                    var url = env.HUB_ASSETS_BASE_URL + Helpers.RemoveCollection(record.Id);
                    uploadRecordService.UpdateHubPublishSuccess(record, url, attempt);
                    // successfully published to the hub at this stage

                    redactedRecord.Publication.Target.Hub.Url = url;
                }
                catch (Exception ex)
                {
                    attempt.Message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                    Logger.Error($"Could not save record to hub database, GUID={record.Id}", ex);
                    throw;
                }

                try
                {
                    // attempt to index the record but don't break downstream processing if this doesn't work
                    hubService.Index(redactedRecord);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Tried but failed to index the record in the hub, GUID={record.Id}", ex);
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
                
                try
                {
                    uploadRecordService.UpdateGovPublishAttempt(record, attempt);
                    db.SaveChanges();

                    var redactedRecord = recordRedactor.RedactRecord(record); // this isn't saved back to the db
                    govService.UploadGeminiXml(Helpers.RemoveCollectionFromId(redactedRecord));
                    govService.UpdateDguIndex(Helpers.RemoveCollectionFromId(redactedRecord));

                    uploadRecordService.UpdateGovPublishSuccess(record, attempt);
                }
                catch (Exception ex)
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
