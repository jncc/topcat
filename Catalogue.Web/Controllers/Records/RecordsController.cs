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

        public Record Get(Guid id, bool clone = false)
        {
            if (id == Guid.Empty) // a nice empty record for making a new one
                return MakeNewRecord();
            else if (clone)
                return Clone(db.Load<Record>(id));
            else
                return db.Load<Record>(id);
        }

        private Record Clone(Record record)
        {
            var clonedRecord = record.Copy();

            clonedRecord.Id = Guid.Empty;
            clonedRecord.Path = String.Empty;
            clonedRecord.Gemini.Title = String.Empty;

            return clonedRecord;
        }

        // PUT api/records/57d34691-9064-4c1e-90a7-7b0c112daa8d (update/replace a record)

        public RecordServiceResult Put(Guid id, [FromBody]Record record)
        {
            SetFooterForUpdatedRecord(record);

            var result = service.Update(record);

            if (result.Record.Id != id) throw new Exception("The ID of the record does not match that supplied to the put method");

            if (result.Success)
                db.SaveChanges();

            return result;
        }

        public RecordServiceResult Post([FromBody] Record record)
        {
            record.Id = Guid.NewGuid();

            SetFooterForNewlyCreatedRecord(record);

            var result = service.Insert(record);

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
                        m.ResponsibleOrganisation = new ResponsibleParty
                            {
                                Name = "Joint Nature Conservation Committee (JNCC)",
                                Email = "data@jncc.gov.uk",
                                Role = "distributor",
                            };
                        m.MetadataDate = Clock.NowUtc;
                        m.MetadataPointOfContact = new ResponsibleParty
                            {
                                Name = user.User.DisplayName,
                                Email = user.User.Email,
                                Role = "author", // it's a new record, so let's suppose the user must be the metadata author
                            };
                    }),
                Review = DateTime.Now.AddYears(3) // arbitrarily decided to default to 3 years from now
            };
        }

        private void SetFooterForNewlyCreatedRecord(Record record)
        {
            record.Footer = new Footer
            {
                CreatedOnUtc = Clock.NowUtc,
                CreatedBy = user.User.DisplayName
            };

            SetFooterForUpdatedRecord(record);
        }

        private void SetFooterForUpdatedRecord(Record record)
        {
            record.Footer.ModifiedOnUtc = Clock.NowUtc;
            record.Footer.ModifiedBy = user.User.DisplayName;
        }
    }
}

