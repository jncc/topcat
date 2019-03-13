using System;
using System.Linq;
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
using NUnit.Framework.Constraints;

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
            redactedRecord.Gemini.Keywords.Count.Should().Be(3);
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
            redactedRecord.Gemini.Keywords.Count.Should().Be(2);
            redactedRecord.Gemini.Keywords.Any(k => k.Value.Equals("A vocabless keyword")).Should().BeTrue();
            redactedRecord.Gemini.Keywords.Any(k => k.Value.Equals("Keyword with vocab")).Should().BeFalse();
            redactedRecord.Gemini.Keywords.Any(k => k.Value.Equals("Another keyword with vocab")).Should().BeTrue();
        }
    }
}
