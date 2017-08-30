using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Utilities.Text;
using Catalogue.Utilities.Time;
using Raven.Client;
using static Catalogue.Utilities.Text.WebificationUtility;

namespace Catalogue.Robot.Publishing.OpenData
{
    public class RobotUploader
    {
        private readonly IDocumentSession db;
        private readonly IOpenDataPublishingUploadService uploadService;
        private readonly IOpenDataUploadHelper uploadHelper;

        public RobotUploader(IDocumentSession db, IOpenDataPublishingUploadService uploadService, IOpenDataUploadHelper uploadHelper)
        {
            this.db = db;
            this.uploadService = uploadService;
            this.uploadHelper = uploadHelper;
        }

        public void Upload(List<Record> records)
        {
            var userInfo = new UserInfo
            {
                DisplayName = "Robot Test Uploader",
                Email = "testemail@company.com"
            };

            foreach (Record record in records)
            {
                Console.WriteLine("Uploading " + record.Gemini.Title);
                UploadRecord(record, userInfo, false);
            }
        }

        private void UploadRecord(Record record, UserInfo userInfo, bool metadataOnly)
        {
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            uploadService.UpdateLastAttempt(record, attempt, userInfo);
            db.SaveChanges();

            bool alternativeResources = record.Publication != null && record.Publication.OpenData != null && record.Publication.OpenData.Resources != null && record.Publication.OpenData.Resources.Any();
            bool corpulent = record.Gemini.Keywords.Any(k => k.Vocab == "http://vocab.jncc.gov.uk/metadata-admin" && k.Value == "Corpulent");

            try
            {
                if (alternativeResources)
                {
                    // upload the alternative resources; don't touch the resource locator
                    uploadHelper.UploadAlternativeResources(record, metadataOnly);
                }
                else
                {
                    if (corpulent)
                    {
                        // set the resource locator to the download request page; don't upload any resources
                        if (record.Gemini.ResourceLocator.IsBlank()) // arguably should always do it actually
                            uploadService.UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(record);
                    }
                    else if (record.Gemini.ResourceLocator.IsBlank() || record.Gemini.ResourceLocator.Contains(uploadHelper.GetHttpRootUrl()))
                    {
                        // "normal" case - if the resource locator is blank or already data.jncc.gov.uk
                        // upload the resource pointed at by record.Path, and update the resource locator to match
                        uploadHelper.UploadDataFile(record.Id, record.Path, metadataOnly);
                        string dataHttpPath = uploadHelper.GetHttpRootUrl() + "/" + GetUnrootedDataPath(record.Id, record.Path);
                        uploadService.UpdateResourceLocatorToMatchMainDataFile(record, dataHttpPath);
                    }
                    else
                    {
                        // do nothing - don't change the resource locator, don't upload anything
                        Console.WriteLine("Deferring to existing resource locator - not uploading.");
                    }
                }

                uploadHelper.UploadMetadataDocument(record);
                uploadHelper.UploadWafIndexDocument(record);

                record.Publication.OpenData.LastSuccess = attempt;
            }
            catch (WebException ex)
            {
                string message = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "");
                Console.WriteLine(message);
                attempt.Message = message;
            }

            // commit the changes - to both the record (resource locator may have changed) and the attempt object
            db.SaveChanges();
        }
    }
}
