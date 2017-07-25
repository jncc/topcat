using Catalogue.Data.Model;
using Catalogue.Gemini.Spatial;
using Catalogue.Utilities.Text;
using Catalogue.Utilities.Time;
using Raven.Client;
using System;
using System.Linq;

namespace Catalogue.Data.Write
{
    public interface IRecordService
    {
        RecordServiceResult Insert(Record record);
        RecordServiceResult Update(Record record);
    }

    public class RecordService : IRecordService
    {
        private readonly IDocumentSession db;
        private readonly IRecordValidator validator;

        public RecordService(IDocumentSession db, IRecordValidator validator)
        {
            this.db = db;
            this.validator = validator;
        }

        public RecordServiceResult Insert(Record record)
        {
            return Upsert(record);
        }

        public RecordServiceResult Update(Record record)
        {
            if (record.ReadOnly)
                throw new InvalidOperationException("Cannot update a read-only record.");

            return Upsert(record);
        }

        internal RecordServiceResult Upsert(Record record)
        {
            CorrectlyOrderKeywords(record);
            StandardiseUnconditionalUseConstraints(record);
            UpdateMetadataDateToNow(record);
            SetMetadataPointOfContactRoleToOnlyAllowedValue(record);

            var validation = validator.Validate(record);

            if (!validation.Errors.Any())
            {
                PerformDenormalizations(record);
                db.Store(record);
            }

            return new RecordServiceResult
                {
                    Record = record,
                    Validation = validation,
                };
        }

        void SetMetadataPointOfContactRoleToOnlyAllowedValue(Record record)
        {
            // the point of contact must be the point of contact (gemini validation)
            record.Gemini.MetadataPointOfContact.Role = "pointOfContact";
        }

        void PerformDenormalizations(Record record)
        {
            // we store the bounding box as wkt so we can index it
            if (!BoundingBoxUtility.IsBlank(record.Gemini.BoundingBox))
                record.Wkt = BoundingBoxUtility.ToWkt(record.Gemini.BoundingBox);
        }

        void CorrectlyOrderKeywords(Record record)
        {
            record.Gemini.Keywords = record.Gemini.Keywords
                .OrderByDescending(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-domain")
                .ThenByDescending(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-category")
                .ThenByDescending(k => k.Vocab.IsNotBlank())
                .ThenBy(k => k.Vocab)
                .ThenBy(k => k.Value)
                .ToList();
        }

        void StandardiseUnconditionalUseConstraints(Record record)
        {
            const string unconditional = "no conditions apply";

            if (record.Gemini.UseConstraints.IsNotBlank() && record.Gemini.UseConstraints.ToLowerInvariant().Trim() == unconditional)
                record.Gemini.UseConstraints = unconditional;
        }

        void UpdateMetadataDateToNow(Record record)
        {
            record.Gemini.MetadataDate = Clock.NowUtc;
        }
    }

    public class RecordServiceResult
    {
        public ValidationResult<Record> Validation { get; set; }
        public bool Success { get { return !Validation.Errors.Any(); } }

        /// <summary>
        /// The (possibly modified) record that was submitted.
        /// </summary>
        public Record Record { get; set; }

        /// <summary>
        /// A convenience result for tests, etc.
        /// </summary>
        public static readonly RecordServiceResult SuccessfulResult = new RecordServiceResult { Validation = new ValidationResult<Record>() };
    }
}
