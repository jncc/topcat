using Catalogue.Utilities.Logging;
using Catalogue.Data.Model;
using System;
using Catalogue.Utilities.Time;
using log4net;
using Raven.Client.Documents.Session;
using static Catalogue.Data.Write.RecordServiceHelper;

namespace Catalogue.Data.Write
{
    public class PublishingUploadRecordService : IPublishingUploadRecordService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishingUploadRecordService));

        private readonly IDocumentSession db;
        private readonly IRecordValidator validator;

        public PublishingUploadRecordService(IDocumentSession db, IRecordValidator validator)
        {
            this.db = db;
            this.validator = validator;
        }

        public void UpdateLastAttempt(Record record, PublicationAttempt attempt)
        {
            record.Publication.OpenData.LastAttempt = attempt;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }

        public void UpdateLastSuccess(Record record, PublicationAttempt attempt)
        {
            record.Publication.OpenData.LastSuccess = attempt;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }
        
        public void UpdatePublishedUrlForResource(Resource resource, string dataHttpPath)
        {
            // update the resource locator to be the data file
            resource.PublishedUrl = dataHttpPath;
            Logger.Info(string.Format("PublishedUrl for resource {0} updated to point to: {1}", resource.Path, dataHttpPath));
        }

        private void UpdateMetadataDate(Record record, DateTime metadataDate)
        {
            record.Gemini.MetadataDate = metadataDate;
        }
    }
}
