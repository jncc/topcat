using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Utilities.Text;
using Catalogue.Utilities.Time;
using log4net;
using Raven.Client;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using static Catalogue.Utilities.Text.WebificationUtility;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class RobotUploader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RobotUploader));

        private readonly IDocumentSession db;
        private readonly IOpenDataPublishingUploadRecordService uploadRecordService;
        private readonly IOpenDataUploadHelper uploadHelper;

        public RobotUploader(IDocumentSession db, IOpenDataPublishingUploadRecordService uploadRecordService, IOpenDataUploadHelper uploadHelper)
        {
            this.db = db;
            this.uploadRecordService = uploadRecordService;
            this.uploadHelper = uploadHelper;
        }

        public List<Record> GetRecordsPendingUpload()
        {
            var records = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.Assessed)
                .Where(x => x.SignedOff)
                .Where(x => x.GeminiValidated) // all open data should be gemini-valid - this is a safety
                .Where(x => !x.PublishedSinceLastUpdated)
                .OfType<Record>() //.Select(r => r.Id) // this doesn't work in RavenDB, and doesn't throw!
                .Take(1000) // so take 1000 which is enough for one run
                .ToList() // so materialize the record first
                .Where(r => !r.Publication.OpenData.Paused) //  .Where(x => !x.PublishingIsPaused) on the server doesn't work on live - thanks, ravenDB
                .ToList();

            return records;
        }

        public void Upload(List<Record> records)
        {
            foreach (Record record in records)
            {
                Logger.Info("Uploading record with title: " + record.Gemini.Title);
                UploadRecord(record);
            }
        }

        private void UploadRecord(Record record)
        {
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            uploadRecordService.UpdateLastAttempt(record, attempt);
            db.SaveChanges();

            bool alternativeResources = record.Publication != null && record.Publication.OpenData != null && record.Publication.OpenData.Resources != null && record.Publication.OpenData.Resources.Any();
            bool corpulent = record.Gemini.Keywords.Any(k => k.Vocab == "http://vocab.jncc.gov.uk/metadata-admin" && k.Value == "Corpulent");

            try
            {
                if (alternativeResources)
                {
                    // upload the alternative resources; don't touch the resource locator
                    uploadHelper.UploadAlternativeResources(record);
                }
                else
                {
                    if (corpulent)
                    {
                        // set the resource locator to the download request page; don't upload any resources
                        if (record.Gemini.ResourceLocator.IsBlank()) // arguably should always do it actually
                            uploadRecordService.UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(record);
                    }
                    else if (record.Gemini.ResourceLocator.IsBlank() || record.Gemini.ResourceLocator.Contains(uploadHelper.GetHttpRootUrl()))
                    {
                        // "normal" case - if the resource locator is blank or already data.jncc.gov.uk
                        // upload the resource pointed at by record.Path, and update the resource locator to match
                        uploadHelper.UploadDataFile(record.Id, record.Path);
                        string dataHttpPath = uploadHelper.GetHttpRootUrl() + "/" + GetUnrootedDataPath(record.Id, record.Path);
                        uploadRecordService.UpdateResourceLocatorToMatchMainDataFile(record, dataHttpPath);
                    }
                    else
                    {
                        // do nothing - don't change the resource locator, don't upload anything
                        Logger.Info("Deferring to existing resource locator - not uploading.");
                    }
                }

                uploadHelper.UploadMetadataDocument(record);
                uploadHelper.UploadWafIndexDocument(record);

                uploadRecordService.UpdateLastSuccess(record, attempt);
            }
            catch (WebException ex)
            {
                string message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                attempt.Message = message;
                Logger.Error("Upload failed for record with GUID="+record.Id, ex);
            }

            // commit the changes - to both the record (resource locator may have changed) and the attempt object
            db.SaveChanges();
        }
    }
}
