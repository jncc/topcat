using Catalogue.Data.Model;
using Catalogue.Utilities.Logging;
using log4net;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
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

        public void UpdateDataPublishAttempt(Record record, PublicationAttempt attempt)
        {
            if (record.Publication.Data == null)
            {
                record.Publication.Data = new DataInfo();
            }
            record.Publication.Data.LastAttempt = attempt;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }

        public void UpdateDataPublishSuccess(Record record, List<Resource> resources, PublicationAttempt attempt)
        {
            record.Publication.Data.LastSuccess = attempt;
            record.Publication.Data.Resources = resources;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }

        public void UpdateGovPublishAttempt(Record record, PublicationAttempt attempt)
        {
            if (record.Publication.Target.Gov == null)
            {
                record.Publication.Target.Gov = new GovPublicationInfo();
            }
            record.Publication.Target.Gov.LastAttempt = attempt;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }

        public void UpdateGovPublishSuccess(Record record, PublicationAttempt attempt)
        {
            record.Publication.Target.Gov.LastSuccess = attempt;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }

        public void UpdateHubPublishAttempt(Record record, PublicationAttempt attempt)
        {
            if (record.Publication.Target.Hub == null)
            {
                record.Publication.Target.Hub = new HubPublicationInfo();
            }
            record.Publication.Target.Hub.LastAttempt = attempt;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }

        public void UpdateHubPublishSuccess(Record record, string hubUrl, PublicationAttempt attempt)
        {
            record.Publication.Target.Hub.LastSuccess = attempt;
            record.Publication.Target.Hub.Url = hubUrl;
            UpdateMetadataDate(record, attempt.DateUtc);

            var recordServiceResult = Upsert(record, db, validator);
            if (!recordServiceResult.Success)
            {
                var e = new Exception("Error while saving upload changes.");
                e.LogAndThrow(Logger);
            }
        }

        private void UpdateMetadataDate(Record record, DateTime metadataDate)
        {
            record.Gemini.MetadataDate = metadataDate;
        }
    }
}
