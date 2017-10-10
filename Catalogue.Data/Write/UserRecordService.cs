using System;
using Catalogue.Data.Model;
using Catalogue.Utilities.Time;
using Raven.Client;

namespace Catalogue.Data.Write
{
    public class UserRecordService : RecordService, IUserRecordService
    {
        public UserRecordService(IDocumentSession db, IRecordValidator validator) : base(db, validator)
        {
        }

        public RecordServiceResult Insert(Record record, UserInfo user)
        {
            SetFooterForNewlyCreatedRecord(record, user);
            UpdateMetadataDate(record);

            return Upsert(record);
        }

        public RecordServiceResult Update(Record record, UserInfo user)
        {
            if (record.ReadOnly)
                throw new InvalidOperationException("Cannot update a read-only record.");

            SetFooterForUpdatedRecord(record, user);
            UpdateMetadataDate(record);

            return Upsert(record);
        }

        private void SetFooterForNewlyCreatedRecord(Record record, UserInfo userInfo)
        {
            var currentTime = Clock.NowUtc;
            record.Footer = new Footer
            {
                CreatedOnUtc = currentTime,
                CreatedByUser = userInfo,
                ModifiedOnUtc = currentTime,
                ModifiedByUser = userInfo
            };
        }

        private void SetFooterForUpdatedRecord(Record record, UserInfo userInfo)
        {
            record.Footer.ModifiedOnUtc = Clock.NowUtc;
            record.Footer.ModifiedByUser = userInfo;
        }

        private void UpdateMetadataDate(Record record)
        {
            record.Gemini.MetadataDate = Clock.NowUtc;
        }
    }
}