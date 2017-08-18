using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using Catalogue.Utilities.Time;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Catalogue.Robot.Publishing.OpenData;
using static Catalogue.Utilities.Text.WebificationUtility;

namespace Catalogue.Data.Write
{
    public class OpenDataPublishingService : IOpenDataPublishingService
    {
        private readonly IDocumentSession db;
        private readonly IRecordService recordService;
        private readonly IOpenDataUploadService uploadService;

        public OpenDataPublishingService(IDocumentSession db, IRecordService recordService, IOpenDataUploadService uploadService)
        {
            this.db = db;
            this.recordService = recordService;
            this.uploadService = uploadService;
        }

        public void SignOff(Record record, OpenDataSignOffInfo signOffInfo)
        {
            if (record.Publication?.OpenData == null)
                throw new Exception("Couldn't sign-off record for publication. Assessment not completed.");

            var openDataInfo = record.Publication.OpenData;

            if (openDataInfo?.SignOff != null && openDataInfo.SignOff.DateUtc != DateTime.MinValue)
                throw new Exception("The record has already been signed off and cannot be signed off again.");

            if (openDataInfo?.Assessment != null && !openDataInfo.Assessment.Completed)
                throw new Exception("Couldn't sign-off record for publication. Assessment not completed.");

            openDataInfo.SignOff = signOffInfo;

            var recordServiceResult = recordService.Update(record, signOffInfo.User);
            if (recordServiceResult.Success)
            {
                db.SaveChanges();
            }
            else
            {
                throw new Exception("Error while saving sign off changes.");
            }
        }

        public Record Assess(Record record, OpenDataAssessmentInfo assessmentInfo)
        {
            if (!record.Validation.Equals(Validation.Gemini))
            {
                throw new Exception("Validation level must be Gemini.");
            }

            if (record.Publication == null)
            {
                record.Publication = new PublicationInfo();
            }

            if (record.Publication.OpenData == null)
            {
                record.Publication.OpenData = new OpenDataPublicationInfo
                {
                    Assessment = new OpenDataAssessmentInfo()
                };
            }

            var assessment = record.Publication.OpenData.Assessment;
            if (assessment != null && assessment.Completed)
            {
                throw new Exception("Assessment has already been completed.");
            }

            record.Publication.OpenData.Assessment = assessmentInfo;

            var recordServiceResult = recordService.Update(record, assessmentInfo.CompletedByUser);
            if (!recordServiceResult.Success)
            {
                throw new Exception("Error while saving assessment changes.");
            }

            db.SaveChanges();

            return recordServiceResult.Record;
        }

        public void Upload(Record record, UserInfo userInfo, bool metadataOnly)
        {
            var attempt = new PublicationAttempt { DateUtc = Clock.NowUtc };
            UpdateLastAttempt(record, attempt, userInfo);

            bool alternativeResources = record.Publication != null && record.Publication.OpenData != null && record.Publication.OpenData.Resources != null && record.Publication.OpenData.Resources.Any();
            bool corpulent = record.Gemini.Keywords.Any(k => k.Vocab == "http://vocab.jncc.gov.uk/metadata-admin" && k.Value == "Corpulent");

            try
            {
                if (alternativeResources)
                {
                    // upload the alternative resources; don't touch the resource locator
                    uploadService.UploadAlternativeResources(record, metadataOnly);
                }
                else
                {
                    if (corpulent)
                    {
                        // set the resource locator to the download request page; don't upload any resources
                        if (record.Gemini.ResourceLocator.IsBlank()) // arguably should always do it actually
                            UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(record);
                    }
                    else if (record.Gemini.ResourceLocator.IsBlank() || record.Gemini.ResourceLocator.Contains(uploadService.GetHttpRootUrl()))
                    {
                        // "normal" case - if the resource locator is blank or already data.jncc.gov.uk
                        // upload the resource pointed at by record.Path, and update the resource locator to match
                        uploadService.UploadDataFile(record.Id, record.Path, metadataOnly);
                        UpdateResourceLocatorToMatchMainDataFile(record);
                    }
                    else
                    {
                        // do nothing - don't change the resource locator, don't upload anything
                        Console.WriteLine("Deferring to existing resource locator - not uploading.");
                    }
                }
                
                uploadService.UploadMetadataDocument(record);
                uploadService.UploadWafIndexDocument(record);

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

        public List<Record> GetRecordsPendingUpload()
        {
            throw new NotImplementedException();
        }

        private void UpdateLastAttempt(Record record, PublicationAttempt attempt, UserInfo userInfo)
        {
            // save a not-yet-successful attempt to begin with
            record.Publication.OpenData.LastAttempt = attempt;

            var recordServiceResult = recordService.Update(record, userInfo);
            if (!recordServiceResult.Success)
            {
                throw new Exception("Error while saving upload changes.");
            }

            db.SaveChanges();
        }

        void UpdateTheResourceLocatorToBeTheOpenDataDownloadPage(Record record)
        {
            // this is a big dataset so just link to a webpage
            string jnccWebDownloadPage = "http://jncc.defra.gov.uk/opendata";
            record.Gemini.ResourceLocator = jnccWebDownloadPage;
            Console.WriteLine("ResourceLocator updated to point to open data request webpage.");
        }

        void UpdateResourceLocatorToMatchMainDataFile(Record record)
        {
            // update the resource locator to be the data file
            string dataHttpPath = uploadService.GetHttpRootUrl() + "/" + GetUnrootedDataPath(record.Id, record.Path);
            record.Gemini.ResourceLocator = dataHttpPath;
            Console.WriteLine("ResourceLocator updated to point to the data file.");
        }
    }
}