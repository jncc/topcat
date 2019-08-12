using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Clone;
using System.Collections.Generic;

namespace Catalogue.Robot.Publishing
{
    public interface IRecordRedactor
    {
        Record RedactRecord(Record record);
    }

    public class RecordRedactor : IRecordRedactor
    {
        private readonly IVocabQueryer queryer;

        public RecordRedactor(IVocabQueryer queryer)
        {
            this.queryer = queryer;
        }

        public Record RedactRecord(Record record)
        {
            //todo: record clone here
            var redactedRecord = RedactResponsibleOrganisation(record);
            redactedRecord = RedactMetadataContact(redactedRecord);
            redactedRecord = RedactKeywords(redactedRecord);
            redactedRecord = RedactImage(redactedRecord);

            return redactedRecord;
        }

        private Record RedactResponsibleOrganisation(Record record)
        {
            if (record.Gemini.ResourceType.Equals("publication"))
            {
                return record.With(r =>
                {
                    r.Gemini.ResponsibleOrganisation.Name = "Communications, JNCC";
                    r.Gemini.ResponsibleOrganisation.Email = "comms@jncc.gov.uk";
                });
            }

            return record.With(r =>
            {
                r.Gemini.ResponsibleOrganisation.Name = "Digital and Data Solutions, JNCC";
                r.Gemini.ResponsibleOrganisation.Email = "data@jncc.gov.uk";
            });
        }

        private Record RedactMetadataContact(Record record)
        {
            if (record.Gemini.ResourceType.Equals("publication"))
            {
                return record.With(r =>
                {
                    r.Gemini.MetadataPointOfContact.Name = "Communications, JNCC";
                    r.Gemini.MetadataPointOfContact.Email = "comms@jncc.gov.uk";
                });
            }

            return record.With(r =>
            {
                r.Gemini.MetadataPointOfContact.Name = "Digital and Data Solutions, JNCC";
                r.Gemini.MetadataPointOfContact.Email = "data@jncc.gov.uk";
            });
        }

        private Record RedactKeywords(Record record)
        {
            var redactedKeywords = new List<MetadataKeyword>();

            foreach (var keyword in record.Gemini.Keywords)
            {
                if (!string.IsNullOrWhiteSpace(keyword.Vocab))
                {
                    var vocab = queryer.GetVocab(keyword.Vocab);

                    if (vocab.Publishable)
                    {
                        redactedKeywords.Add(keyword);
                    }
                }
            }

            return record.With(r =>
            {
                r.Gemini.Keywords = redactedKeywords;
            });
        }

        private Record RedactImage(Record record)
        {
            // not technically a redaction, but we only want images for publications for now

            if (!record.Gemini.ResourceType.Equals("publication"))
            {
                return record.With(r => r.Image = null);
            }

            return record;
        }
    }
}
