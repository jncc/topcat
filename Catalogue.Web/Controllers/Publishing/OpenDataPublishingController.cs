using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Data;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Utilities.Time;
using Catalogue.Web.Account;
using Catalogue.Web.Security;
using Raven.Client.Documents.Session;

namespace Catalogue.Web.Controllers.Publishing
{
    public class OpenDataPublishingController : ApiController
    {
        readonly IDocumentSession db;
        readonly IOpenDataPublishingRecordService openDataPublishingRecordService;
        readonly IUserContext user;

        public OpenDataPublishingController(IDocumentSession db, IOpenDataPublishingRecordService openDataPublishingRecordService, IUserContext user)
        {
            this.db = db;
            this.openDataPublishingRecordService = openDataPublishingRecordService;
            this.user = user;
        }

        [HttpGet, Route("api/publishing/opendata/summary")]
        public SummaryRepresentation Summary()
        {
            var query = db.Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>();

            return new SummaryRepresentation
                {
                    CountOfPublishedSinceLastUpdated = query.Count(x => x.PublishedSinceLastUpdated),
                    CountOfNotYetPublishedSinceLastUpdated = query.Count(x => !x.PublishedSinceLastUpdated && x.SignedOff),
                    CountOfPublicationNeverAttempted = query.Count(x => x.PublicationNeverAttempted),
                    CountOfLastPublicationAttemptWasUnsuccessful = query.Count(x => x.LastPublicationAttemptWasUnsuccessful),
                    CountOfPendingSignOff = query.Count(x => x.Assessed && !x.SignedOff)
            };
        }

        [HttpPut, Route("api/publishing/opendata/assess")]
        public object Assess(AssessmentRequest assessmentRequest)
        {
            var record = db.Load<Record>(Helpers.AddCollection(assessmentRequest.Id));
            var assessmentInfo = new OpenDataAssessmentInfo
            {
                Completed = true,
                CompletedByUser = new UserInfo
                {
                    DisplayName = user.User.DisplayName,
                    Email = user.User.Email
                },
                CompletedOnUtc = Clock.NowUtc,
                InitialAssessmentWasDoneOnSpreadsheet = record.Publication?.Assessment?.InitialAssessmentWasDoneOnSpreadsheet == true
            };

            var updatedRecord = openDataPublishingRecordService.Assess(record, assessmentInfo);

            db.SaveChanges();

            updatedRecord.Record = Helpers.RemoveCollectionFromId(updatedRecord.Record);
            return updatedRecord;
        }

        [HttpPut, Route("api/publishing/opendata/signoff"), AuthorizeOpenDataIao]
        public object SignOff(SignOffRequest signOffRequest)
        {
            var record = db.Load<Record>(Helpers.AddCollection(signOffRequest.Id));
            var signOffInfo = new OpenDataSignOffInfo
            {
                User = new UserInfo
                {
                    DisplayName = user.User.DisplayName,
                    Email = user.User.Email
                },
                DateUtc = Clock.NowUtc,
                Comment = signOffRequest.Comment
            };

            var updatedRecord = openDataPublishingRecordService.SignOff(record, signOffInfo);

            db.SaveChanges();

            updatedRecord.Record = Helpers.RemoveCollectionFromId(updatedRecord.Record);
            return updatedRecord;
        }

        [HttpGet, Route("api/publishing/opendata/pendingsignoff")]
        public List<RecordRepresentation> PendingSignOff(int p = 1)
        {
            var query = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.Assessed && !x.SignedOff);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/opendata/pendingsignoffcountforcurrentuser")]
        public int PendingSignOffCountForCurrentUser()
        {
            int count = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Count(x => x.Assessed && !x.SignedOff);

            // if user is an IAO, then (for now) they can see all records for sign off
            // if they're not an IAO, it's nothing to do with them, so they have none
            return user.User.IsIaoUser ? count : 0;
        }

        [HttpGet, Route("api/publishing/opendata/publishedsincelastupdated")]
        public List<RecordRepresentation> PublishedSinceLastUpdated(int p = 1)
        {
            var query = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.PublishedSinceLastUpdated);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/opendata/notpublishedsincelastupdated")]
        public List<RecordRepresentation> Pending(int p = 1)
        {
            var query = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => !x.PublishedSinceLastUpdated && x.SignedOff);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/opendata/publicationneverattempted")]
        public List<RecordRepresentation> PublicationNeverAttempted(int p = 1)
        {
            var query = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.PublicationNeverAttempted);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/opendata/lastpublicationattemptwasunsuccessful")]
        public List<RecordRepresentation> LastPublicationAttemptWasUnsuccessful(int p = 1)
        {
            var query = db
                .Query<RecordsWithOpenDataPublicationInfoIndex.Result, RecordsWithOpenDataPublicationInfoIndex>()
                .Where(x => x.LastPublicationAttemptWasUnsuccessful);

            return GetRecords(query, p);
        }

        List<RecordRepresentation> GetRecords(IQueryable<RecordsWithOpenDataPublicationInfoIndex.Result> query, int p)
        {
            int take = 1000; // can change this when paging is implemented in the UI
            int skip = (p - 1) * take;

            var records = query
                .Skip(skip)
                .Take(take)
                .OfType<Record>()
                .ToList()
                .Select(r => new RecordRepresentation
                {
                    Id = Helpers.RemoveCollection(r.Id),
                    Title = r.Gemini.Title,
                    MetadataDate = r.Gemini.MetadataDate,
                    IsGeminiValid = r.Validation == Validation.Gemini,
                    Gov = r.Publication.Gov,
                })
                .ToList();

            return records;
        }
    }

    public class RecordRepresentation
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime MetadataDate { get; set; }
        public bool IsGeminiValid { get; set; }
        public GovPublicationInfo Gov { get; set; }
    }

    public class SummaryRepresentation
    {
        public int CountOfPublishedSinceLastUpdated { get; set; }
        public int CountOfNotYetPublishedSinceLastUpdated { get; set; }
        public int CountOfPublicationNeverAttempted { get; set; }
        public int CountOfLastPublicationAttemptWasUnsuccessful { get; set; }
        public int CountOfPendingSignOff { get; set; }
    }
}


