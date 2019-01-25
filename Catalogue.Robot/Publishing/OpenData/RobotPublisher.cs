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
using Catalogue.Utilities.Clone;
using Quartz.Util;
using static Catalogue.Utilities.Text.WebificationUtility;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class RobotPublisher
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RobotPublisher));

        private readonly IDocumentSession db;
        private readonly IPublishingUploadRecordService uploadRecordService;
        private readonly IOpenDataUploadHelper uploadHelper;
        private readonly IHubService hubService;

        public RobotPublisher(IDocumentSession db, IPublishingUploadRecordService uploadRecordService, IOpenDataUploadHelper uploadHelper, IHubService hubService)
        {
            this.db = db;
            this.uploadRecordService = uploadRecordService;
            this.uploadHelper = uploadHelper;
            this.hubService = hubService;
        }

        public List<Record> GetRecordsPendingUpload()
        {
            var records = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Customize(x => x.WaitForNonStaleResults())
                .Where(x => x.Assessed)
                .Where(x => x.SignedOff)
                .Where(x => x.GeminiValidated) // all open data should be gemini-valid - this is a safety
                .Where(x => x.Publishable == true)
                .Where(x => !x.PublishedSinceLastUpdated)
                .OfType<Record>() //.Select(r => r.Id) // this doesn't work in RavenDB, and doesn't throw!
                .Take(1000) // so take 1000 which is enough for one run
                .ToList() // so materialize the record first
                .Where(r => !r.Publication.Gov.Paused) //  .Where(x => !x.PublishingIsPaused) on the server doesn't work on live - thanks, ravenDB
                .ToList();

            return records;
        }

        public void PublishRecords(List<Record> records, bool metadataOnly = false)
        {
            foreach (Record record in records)
            {
                Logger.Info($"Starting publishing process for record {record.Id} {record.Gemini.Title}");
                if (!metadataOnly)
                {
                    try
                    {
                        PublishDataFiles(record);
                        PublishHubMetadata(record);
                        PublishGovMetadata(record);
                    }
                    catch (WebException ex)
                    {
                        Logger.Error($"Could not complete publishing process for record with GUID={record.Id}", ex);
                    }
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
                        if (IsFileResource(resource))
                        {
                            Logger.Info($"Resource {resource.Path} is a file - starting upload process");
                            uploadHelper.UploadDataFile(Helpers.RemoveCollection(record.Id), resource.Path);
                            string dataHttpPath = uploadHelper.GetHttpRootUrl() + "/" +
                                                  GetUnrootedDataPath(Helpers.RemoveCollection(record.Id),
                                                      resource.Path);
                            resource.PublishedUrl = dataHttpPath;
                        }
                        else
                        {
                            Logger.Info($"Resource {resource.Path} is a URL - nothing to do");
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
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            uploadRecordService.UpdateHubPublishAttempt(record, attempt);
            db.SaveChanges();

            try
            {
                hubService.Upsert(record);

                var url = "http://hub.jncc.gov.uk/asset/" + Helpers.RemoveCollection(record.Id);
                uploadRecordService.UpdateHubPublishSuccess(record, url, attempt);

                hubService.Index(record);
            }
            catch (WebException ex)
            {
                attempt.Message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logger.Error($"Could not save record to hub database, GUID={record.Id}", ex);
                throw;
            }
        }

        public void PublishGovMetadata(Record record)
        {
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            uploadRecordService.UpdateGovPublishAttempt(record, attempt);
            db.SaveChanges();

            try
            {
                uploadHelper.UploadMetadataDocument(Helpers.RemoveCollectionFromId(record));
                uploadHelper.UploadWafIndexDocument(Helpers.RemoveCollectionFromId(record));

                uploadRecordService.UpdateGovPublishSuccess(record, attempt);
            }
            catch (WebException ex)
            {
                attempt.Message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logger.Error($"DGU metadata transfer failed for record with GUID={record.Id}", ex);
                throw;
            }
        }

        private bool IsFileResource(Resource resource)
        {
            var isFilePath = false;
            if (Uri.TryCreate(resource.Path, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    isFilePath = true;
                }
            }

            return isFilePath;
        }
    }
}
