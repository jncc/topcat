using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Robot.Publishing;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    public class record_redactor_specs
    {
        private Record record = new Record().With(r =>
        {
            r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
            r.Path = @"X:\path\to\working\folder";
            r.Validation = Validation.Gemini;
            r.Gemini = Library.Example().With(m =>
            {
                m.Keywords = new StringPairList
                    {
                        { "", "A vocabless keyword" },
                        {"http://vocab.jncc.gov.uk/some-vocab", "Keyword with vocab"},
                        {"http://vocab.jncc.gov.uk/another-vocab", "Another keyword with vocab"}
                    }
                    .ToKeywordList();
                m.ResponsibleOrganisation = new ResponsibleParty
                {
                    Name = "Test user",
                    Email = "test.user@jncc.gov.uk",
                    Role = "owner"
                };
                m.MetadataPointOfContact = new ResponsibleParty
                {
                    Name = "Another test user",
                    Email = "another.test.user@jncc.gov.uk",
                    Role = "pointOfContact",
                };
            });
            r.Footer = new Footer();
        });

        [Test]
        public void redact_record_with_publishable_keywords()
        {
            var queryerMock = new Mock<IVocabQueryer>();
            var redactor = new RecordRedactor(queryerMock.Object);
            queryerMock.Setup(x => x.GetVocab("http://vocab.jncc.gov.uk/some-vocab")).Returns(new Vocabulary { Publishable = true });
            queryerMock.Setup(x => x.GetVocab("http://vocab.jncc.gov.uk/another-vocab")).Returns(new Vocabulary { Publishable = true });

            var redactedRecord = redactor.RedactRecord(record);
            redactedRecord.Gemini.ResponsibleOrganisation.Name.Should().Be("Digital and Data Solutions, JNCC");
            redactedRecord.Gemini.ResponsibleOrganisation.Email.Should().Be("data@jncc.gov.uk");
            redactedRecord.Gemini.ResponsibleOrganisation.Role.Should().Be("owner");
            redactedRecord.Gemini.MetadataPointOfContact.Name.Should().Be("Digital and Data Solutions, JNCC");
            redactedRecord.Gemini.MetadataPointOfContact.Email.Should().Be("data@jncc.gov.uk");
            redactedRecord.Gemini.MetadataPointOfContact.Role.Should().Be("pointOfContact");
            redactedRecord.Gemini.Keywords.Count.Should().Be(2);
        }

        [Test]
        public void redact_record_with_unpublishable_keywords()
        {
            var queryerMock = new Mock<IVocabQueryer>();
            var redactor = new RecordRedactor(queryerMock.Object);
            queryerMock.Setup(x => x.GetVocab("http://vocab.jncc.gov.uk/some-vocab")).Returns(new Vocabulary{Publishable = false});
            queryerMock.Setup(x => x.GetVocab("http://vocab.jncc.gov.uk/another-vocab")).Returns(new Vocabulary { Publishable = true });

            var redactedRecord = redactor.RedactRecord(record);
            redactedRecord.Gemini.ResponsibleOrganisation.Name.Should().Be("Digital and Data Solutions, JNCC");
            redactedRecord.Gemini.ResponsibleOrganisation.Email.Should().Be("data@jncc.gov.uk");
            redactedRecord.Gemini.ResponsibleOrganisation.Role.Should().Be("owner");
            redactedRecord.Gemini.MetadataPointOfContact.Name.Should().Be("Digital and Data Solutions, JNCC");
            redactedRecord.Gemini.MetadataPointOfContact.Email.Should().Be("data@jncc.gov.uk");
            redactedRecord.Gemini.MetadataPointOfContact.Role.Should().Be("pointOfContact");
            redactedRecord.Gemini.Keywords.Count.Should().Be(1);
            redactedRecord.Gemini.Keywords.Any(k => k.Value.Equals("A vocabless keyword")).Should().BeFalse();
            redactedRecord.Gemini.Keywords.Any(k => k.Value.Equals("Keyword with vocab")).Should().BeFalse();
            redactedRecord.Gemini.Keywords.Any(k => k.Value.Equals("Another keyword with vocab")).Should().BeTrue();
        }

        [Test]
        public void remove_images_from_datasets_and_services([Values("dataset", "nonGeographicDataset", "service")] string resourceType)
        {
            var nonPublicationRecord = record.With(r =>
            {
                r.Image = new Image
                {
                    Url = "http://an.image.url"
                };
                r.Gemini.ResourceType = resourceType;
            });
            var queryerMock = new Mock<IVocabQueryer>();
            var redactor = new RecordRedactor(queryerMock.Object);
            queryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary { Publishable = true });

            var redactedRecord = redactor.RedactRecord(nonPublicationRecord);
            redactedRecord.Image.Should().BeNull();
        }

        [Test]
        public void keep_images_for_publications()
        {
            var publicationRecord = record.With(r =>
            {
                r.Image = new Image
                {
                    Url = "http://an.image.url"
                };
                r.Gemini.ResourceType = "publication";
            });
            var queryerMock = new Mock<IVocabQueryer>();
            var redactor = new RecordRedactor(queryerMock.Object);
            queryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary { Publishable = true });

            var redactedRecord = redactor.RedactRecord(publicationRecord);
            redactedRecord.Image.Url.Should().Be("http://an.image.url");
        }

        [Test]
        public void use_comms_contact_details_for_publications()
        {
            var publicationRecord = record.With(r =>
            {
                r.Gemini.ResourceType = "publication";
            });

            var queryerMock = new Mock<IVocabQueryer>();
            var redactor = new RecordRedactor(queryerMock.Object);
            queryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary { Publishable = true });

            var redactedRecord = redactor.RedactRecord(publicationRecord);
            redactedRecord.Gemini.ResponsibleOrganisation.Name.Should().Be("Communications, JNCC");
            redactedRecord.Gemini.ResponsibleOrganisation.Email.Should().Be("comms@jncc.gov.uk");
            redactedRecord.Gemini.MetadataPointOfContact.Name.Should().Be("Communications, JNCC");
            redactedRecord.Gemini.MetadataPointOfContact.Email.Should().Be("comms@jncc.gov.uk");
        }
    }
}
