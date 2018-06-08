using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Time;
using Catalogue.Web.Account;
using Raven.Client;
using System;
using System.Web.Http;
using Catalogue.Data;
using Catalogue.Data.Extensions;
using Catalogue.Data.Query;
using NUnit.Framework;
using Raven.Client.Documents.Session;

namespace Catalogue.Web.Controllers.Records
{
    public class RecordsController : ApiController
    {
        readonly IDocumentSession db;
        readonly IRecordService service;
        readonly IUserContext user;

        public RecordsController(IRecordService service, IDocumentSession db, IUserContext user)
        {
            this.service = service;
            this.db = db;
            this.user = user;
        }

        // GET api/records/57d34691-9064-4c1e-90a7-7b0c112daa8d (get a record)

        public object Get(Guid id, bool clone = false)
        {
            Record record;

            if (id == Guid.Empty)
                record = MakeNewRecord(); // a nice empty record for making a new one
            else if (clone)
                record = Clone(db.Load<Record>(Helpers.GetRecordId(id)));
            else
                record = db.Load<Record>(Helpers.GetRecordId(id));

            return new RecordOutputModel
            {
                Record = record,
                RecordState = new RecordState
                {
                    OpenDataPublishingState = new OpenDataPublishingState
                    {
                        AssessedAndUpToDate = record.IsAssessedAndUpToDate(),
                        SignedOffAndUpToDate = record.IsSignedOffAndUpToDate(),
                        UploadedAndUpToDate = record.IsUploadedAndUpToDate()
                    }
                }
            };
        }

        private Record Clone(Record record)
        {
            var clonedRecord = record.Copy();

            clonedRecord.Id = Guid.Empty;
            clonedRecord.Path = String.Empty;
            clonedRecord.Gemini.Title = String.Empty;
            clonedRecord.Publication = null;

            return clonedRecord;
        }

        // PUT api/records/57d34691-9064-4c1e-90a7-7b0c112daa8d (update/replace a record)

        public object Put(Guid id, [FromBody]Record record)
        {
            var userInfo = new UserInfo
            {
                DisplayName = user.User.DisplayName,
                Email = user.User.Email
            };

            var result = service.Update(record, userInfo);

            if (result.Record.Id != id) throw new Exception("The ID of the record does not match that supplied to the put method");

            if (result.Success)
                db.SaveChanges();

            return result;
        }

        public object Post([FromBody] Record record)
        {
            record.Id = Guid.NewGuid();

            var userInfo = new UserInfo
            {
                DisplayName = user.User.DisplayName,
                Email = user.User.Email
            };

            var result = service.Insert(record, userInfo);

            if (result.Success)
                db.SaveChanges();

            return result;
        }

        Record MakeNewRecord()
        {
            return new Record
            {
                Id = Guid.Empty,
                Gemini = Library.Blank().With(m =>
                    {
                        m.ResourceType = "dataset";
                        m.MetadataDate = Clock.NowUtc;
                    }),
                Review = DateTime.Now.AddYears(3) // arbitrarily decided to default to 3 years from now
            };
        }
    }
}

