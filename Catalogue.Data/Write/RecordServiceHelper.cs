using Catalogue.Data.Model;
using Catalogue.Gemini.Spatial;
using Catalogue.Utilities.Text;
using System.Linq;
using Catalogue.Data.Extensions;
using Raven.Client.Documents.Session;

namespace Catalogue.Data.Write
{
    public static class RecordServiceHelper
    {
        public static RecordServiceResult Upsert(Record record, IDocumentSession db, IRecordValidator validator)
        {
            CorrectlyOrderKeywords(record);
            StandardiseUnconditionalUseConstraints(record);
            SetMetadataPointOfContactRoleToOnlyAllowedValue(record);
            TrimPublishableResourcePaths(record);

            var validation = validator.Validate(record);

            if (!validation.Errors.Any())
            {
                PerformDenormalizations(record);
                db.Store(record);
            }

            return new RecordServiceResult
                {
                    Record = record,
                    RecordState = new RecordState
                    {
                        PublishingState = new PublishingState {
                            AssessedAndUpToDate = record.IsAssessedAndUpToDate(),
                            SignedOffAndUpToDate = record.IsSignedOffAndUpToDate(),
                            PublishedToHubAndUpToDate = record.IsPublishedToHubAndUpToDate(),
                            PublishedToGovAndUpToDate = record.IsPublishedToGovAndUpToDate(),
                            PublishedAndUpToDate = record.IsPublishedAndUpToDate(),
                            PreviouslyPublishedWithDoi = record.HasPreviouslyBeenPublishedWithDoi()
                        }
                    },
                    Validation = validation
                };
        }

        private static void SetMetadataPointOfContactRoleToOnlyAllowedValue(Record record)
        {
            // the point of contact must be the point of contact (gemini validation)
            record.Gemini.MetadataPointOfContact.Role = "pointOfContact";
        }

        private static void PerformDenormalizations(Record record)
        {
            // we store the bounding box as wkt so we can index it
            if (!BoundingBoxUtility.IsBlank(record.Gemini.BoundingBox))
                record.Wkt = BoundingBoxUtility.ToWkt(record.Gemini.BoundingBox);
        }

        private static void CorrectlyOrderKeywords(Record record)
        {
            record.Gemini.Keywords = record.Gemini.Keywords
                .OrderByDescending(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-domain")
                .ThenByDescending(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-category")
                .ThenByDescending(k => k.Vocab.IsNotBlank())
                .ThenBy(k => k.Vocab)
                .ThenBy(k => k.Value)
                .ToList();
        }

        private static void StandardiseUnconditionalUseConstraints(Record record)
        {
            const string unconditional = "no conditions apply";

            if (record.Gemini.UseConstraints.IsNotBlank() && record.Gemini.UseConstraints.ToLowerInvariant().Trim() == unconditional)
                record.Gemini.UseConstraints = unconditional;
        }

        private static void TrimPublishableResourcePaths(Record record)
        {
            var resources = record?.Publication?.Data?.Resources;

            if (resources != null)
            {
                foreach (var r in resources)
                    r.Path = r.Path.Trim();
            }
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
        public RecordState RecordState { get; set; }

        /// <summary>
        /// A convenience result for tests, etc.
        /// </summary>
        public static readonly RecordServiceResult SuccessfulResult = new RecordServiceResult { Validation = new ValidationResult<Record>() };
    }
}
