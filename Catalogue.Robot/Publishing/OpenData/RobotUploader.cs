using System;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Utilities.Text;
using Catalogue.Utilities.Time;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Catalogue.Data;
using Raven.Client.Documents.Session;
using static Catalogue.Utilities.Text.WebificationUtility;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class RobotUploader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RobotUploader));

        private readonly IDocumentSession db;
        private readonly IPublishingUploadRecordService uploadRecordService;
        private readonly IOpenDataUploadHelper uploadHelper;

        public RobotUploader(IDocumentSession db, IPublishingUploadRecordService uploadRecordService, IOpenDataUploadHelper uploadHelper)
        {
            this.db = db;
            this.uploadRecordService = uploadRecordService;
            this.uploadHelper = uploadHelper;
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
                .Where(r => !r.Publication.OpenData.Paused) //  .Where(x => !x.PublishingIsPaused) on the server doesn't work on live - thanks, ravenDB
                .ToList();

            return records;
        }

        public void Upload(List<Record> records, bool metadataOnly = false)
        {
            foreach (Record record in records)
            {
                Logger.Info("Uploading record with title: " + record.Gemini.Title);
                UploadRecord(record, metadataOnly);
            }
            // commit the changes - to both the record (resource locator may have changed) and the attempt object
            db.SaveChanges();
        }

        private void UploadRecord(Record record, bool metadataOnly)
        {
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            uploadRecordService.UpdateLastAttempt(record, attempt);
            db.SaveChanges();

            try
            {
                if (!metadataOnly)
                {
                    foreach (var resource in record.Publication.OpenData.Resources)
                    {
                        if (IsFileResource(resource))
                        {
                            uploadHelper.UploadDataFile(Helpers.RemoveCollection(record.Id), resource.Path);
                            string dataHttpPath = uploadHelper.GetHttpRootUrl() + "/" +
                                                  GetUnrootedDataPath(Helpers.RemoveCollection(record.Id), resource.Path);
                            uploadRecordService.UpdatePublishedUrlForResource(resource, dataHttpPath);
                        }
                        else
                        {
                            // do nothing
                            Logger.Info("The resource is a URL - no file to upload");
                        }
                    }
                }

                uploadHelper.UploadMetadataDocument(Helpers.RemoveCollectionFromId(record));
                uploadHelper.UploadWafIndexDocument(Helpers.RemoveCollectionFromId(record));

                uploadRecordService.UpdateLastSuccess(record, attempt);
            }
            catch (WebException ex)
            {
                attempt.Message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logger.Error($"Upload failed for record with GUID={record.Id}", ex);
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
