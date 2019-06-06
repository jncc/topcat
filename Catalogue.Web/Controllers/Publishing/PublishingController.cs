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
    public class PublishingController : ApiController
    {
        readonly IDocumentSession db;
        readonly IRecordPublishingService recordPublishingService;
        readonly IUserContext user;

        public PublishingController(IDocumentSession db, IRecordPublishingService recordPublishingService, IUserContext user)
        {
            this.db = db;
            this.recordPublishingService = recordPublishingService;
            this.user = user;
        }

        [HttpGet, Route("api/publishing/summary")]
        public SummaryRepresentation Summary()
        {
            var query = db.Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>();

            return new SummaryRepresentation
                {
                    CountOfPublishedSinceLastUpdated = query.Count(x => x.PublishedToGovSinceLastUpdated || x.PublishedToHubSinceLastUpdated),
                    CountOfNotYetPublishedSinceLastUpdated = query.Count(x => !x.PublishedToGovSinceLastUpdated && !x.PublishedToHubSinceLastUpdated && x.SignedOff),
                    CountOfPublicationNeverAttempted = query.Count(x => x.PublicationNeverAttempted),
                    CountOfPendingSignOff = query.Count(x => x.Assessed && !x.SignedOff)
            };
        }

        [HttpPut, Route("api/publishing/assess")]
        public object Assess(AssessmentRequest assessmentRequest)
        {
            var record = db.Load<Record>(Helpers.AddCollection(assessmentRequest.Id));
            var assessmentInfo = new AssessmentInfo
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

            var updatedRecord = recordPublishingService.Assess(record, assessmentInfo);

            db.SaveChanges();

            updatedRecord.Record = Helpers.RemoveCollectionFromId(updatedRecord.Record);
            return updatedRecord;
        }

        [HttpPut, Route("api/publishing/signoff"), AuthorizeIao]
        public object SignOff(SignOffRequest signOffRequest)
        {
            var record = db.Load<Record>(Helpers.AddCollection(signOffRequest.Id));
            var signOffInfo = new SignOffInfo
            {
                User = new UserInfo
                {
                    DisplayName = user.User.DisplayName,
                    Email = user.User.Email
                },
                DateUtc = Clock.NowUtc,
                Comment = signOffRequest.Comment
            };

            var updatedRecord = recordPublishingService.SignOff(record, signOffInfo);

            db.SaveChanges();

            updatedRecord.Record = Helpers.RemoveCollectionFromId(updatedRecord.Record);
            return updatedRecord;
        }

        [HttpGet, Route("api/publishing/pendingsignoff")]
        public List<RecordRepresentation> PendingSignOff(int p = 1)
        {
            var query = db
                .Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Where(x => x.Assessed && !x.SignedOff);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/pendingsignoffcountforcurrentuser")]
        public int PendingSignOffCountForCurrentUser()
        {
            int count = db
                .Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Count(x => x.Assessed && !x.SignedOff);

            // if user is an IAO, then (for now) they can see all records for sign off
            // if they're not an IAO, it's nothing to do with them, so they have none
            return user.User.IsIaoUser ? count : 0;
        }

        [HttpGet, Route("api/publishing/publishedsincelastupdated")]
        public List<RecordRepresentation> PublishedSinceLastUpdated(int p = 1)
        {
            var query = db
                .Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Where(x => x.PublishedToGovSinceLastUpdated || x.PublishedToHubSinceLastUpdated);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/notpublishedsincelastupdated")]
        public List<RecordRepresentation> Pending(int p = 1)
        {
            var query = db
                .Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Where(x => !x.PublishedToGovSinceLastUpdated && !x.PublishedToHubSinceLastUpdated && x.SignedOff);

            return GetRecords(query, p);
        }

        [HttpGet, Route("api/publishing/publicationneverattempted")]
        public List<RecordRepresentation> PublicationNeverAttempted(int p = 1)
        {
            var query = db
                .Query<RecordsWithPublicationInfoIndex.Result, RecordsWithPublicationInfoIndex>()
                .Where(x => x.PublicationNeverAttempted);

            return GetRecords(query, p);
        }

        List<RecordRepresentation> GetRecords(IQueryable<RecordsWithPublicationInfoIndex.Result> query, int p)
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
                    PublicationInfo = r.Publication
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
        public PublicationInfo PublicationInfo { get; set; }
    }

    public class SummaryRepresentation
    {
        public int CountOfPublishedSinceLastUpdated { get; set; }
        public int CountOfNotYetPublishedSinceLastUpdated { get; set; }
        public int CountOfPublicationNeverAttempted { get; set; }
        public int CountOfPendingSignOff { get; set; }
    }
}


