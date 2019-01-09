using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Spatial;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Data;
using Ninject.Infrastructure.Language;
using Raven.Client.Documents.Session;
using static Catalogue.Tests.TestUserInfo;

namespace Catalogue.Tests.Slow.Catalogue.Data.Write
{
    public class record_service_specs
    {
        [Test]
        public void should_fail_when_record_is_readonly_on_update()
        {
            var service = new RecordService(Mock.Of<IDocumentSession>(), Mock.Of<IRecordValidator>());
            var record = new Record { ReadOnly = true };

            service.Invoking(s => s.Update(record, TestUser))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot update a read-only record.");
        }

        [Test]
        public void should_store_valid_record_in_the_database()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            var record = BasicRecord();
            service.Update(record, TestUser);

            Mock.Get(database).Verify(db => db.Store(record));
        }

        [Test]
        public void should_not_store_invalid_record_in_the_database()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, FailingValidatorStub());

            service.Update(BasicRecord(), TestUser);

            Mock.Get(database).Verify(db => db.Store(It.IsAny<Record>()), Times.Never);
        }

        [Test]
        public void should_return_record_in_result()
        {
            // so we can pass the possibly modified record back to the client
            // without an unnecessary fetch from the database

            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            var record = BasicRecord();
            var result = service.Update(record, TestUser);

            result.Record.Should().Be(record);
        }

        [Test]
        public void should_store_bounding_box_as_wkt()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            var e = Library.Example();
            var record = new Record
            {
                Gemini = e,
                Footer = new Footer()
            };

            service.Update(record, TestUser);

            string expectedWkt = BoundingBoxUtility.ToWkt(e.BoundingBox);
            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Wkt == expectedWkt)));
        }

        [Test]
        public void should_store_empty_bounding_box_as_null_wkt()
        {
            // to avoid raven / lucene indexing errors

            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            service.Update(BasicRecord(), TestUser);

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Wkt == null)));
        }

        [Test]
        public void should_standardise_unconditional_use_constraints()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            var record = BasicRecord().With(r => r.Gemini.UseConstraints = "   No conditions APPLY");
            service.Update(record, TestUser);

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Gemini.UseConstraints == "no conditions apply")));
        }

        [Test]
        public void should_set_security_to_official_by_default()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            service.Update(BasicRecord(), TestUser);

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Security == Security.Official)));
        }

        [Test]
        public void should_save_keywords_in_correct_order()
        {
            // should be sorted by vocab, then value, but with distinguished jncc vocabs first!
            // finally, keywords with no namespace should be last

            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            var record = BasicRecord().With(r =>
            {
                r.Gemini.Keywords = new StringPairList
                {
                    { "a-vocab-beginning-with-a", "bravo" },
                    { "boring-vocab-beginning-with-b", "some-keyword" },
                    { "a-vocab-beginning-with-a", "alpha" },
                    { "http://vocab.jncc.gov.uk/jncc-category", "bravo" },
                    { "http://vocab.jncc.gov.uk/jncc-category", "alpha" },
                    { "http://vocab.jncc.gov.uk/jncc-domain", "some-domain" },
                    { "", "some-keyword" },
                }.ToKeywordList();
            });

            service.Update(record, TestUser);

            var expected = new StringPairList
            {
                { "http://vocab.jncc.gov.uk/jncc-domain", "some-domain" },
                { "http://vocab.jncc.gov.uk/jncc-category", "alpha" },
                { "http://vocab.jncc.gov.uk/jncc-category", "bravo" },
                { "a-vocab-beginning-with-a", "alpha" },
                { "a-vocab-beginning-with-a", "bravo" },
                { "boring-vocab-beginning-with-b", "some-keyword" },
                { "", "some-keyword" },
            }.ToKeywordList();

            Mock.Get(database).Verify(db => db.Store(It.Is((Record r) => r.Gemini.Keywords.IsEqualTo(expected))));
        }

        [Test]
        public void should_give_new_record_a_footer()
        {
            var record = new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m => m.Title = "Footer creation test")
            };
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            var result = service.Insert(record, TestUser);
            var footer = result.Record.Footer;
            footer.Should().NotBeNull();
            footer.CreatedOnUtc.Should().NotBe(DateTime.MinValue);
            footer.CreatedByUser.DisplayName.Should().Be("Test User");
            footer.CreatedByUser.Email.Should().Be("tester@example.com");
            footer.ModifiedOnUtc.Should().NotBe(DateTime.MinValue);
            footer.ModifiedByUser.DisplayName.Should().Be("Test User");
            footer.ModifiedByUser.Email.Should().Be("tester@example.com");
        }

        [Test]
        public void should_update_footer_for_existing_record()
        {
            var recordId = Helpers.AddCollection("4d909f48-4547-4129-a663-bfab64ae97e9");
            var record = new Record
            {
                Id = recordId,
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m => m.Title = "Footer update test"),
                Footer = new Footer
                {
                    CreatedOnUtc = new DateTime(2015, 1, 1, 10, 0, 0),
                    CreatedByUser = new UserInfo
                    {
                        DisplayName = "Creator",
                        Email = "creator@example.com"
                    },
                    ModifiedOnUtc = new DateTime(2015, 1, 1, 10, 0, 0),
                    ModifiedByUser = new UserInfo
                    {
                        DisplayName = "Creator",
                        Email = "creator@example.com"
                    }
                }
            };

            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            var result = service.Update(record, TestUser);
            var footer = result.Record.Footer;
            footer.Should().NotBeNull();
            footer.CreatedOnUtc.Should().Be(new DateTime(2015, 1, 1, 10, 0, 0));
            footer.CreatedByUser.DisplayName.Should().Be("Creator");
            footer.CreatedByUser.Email.Should().Be("creator@example.com");
            footer.ModifiedOnUtc.Should().Be(new DateTime(2015, 1, 1, 12, 0, 0));
            footer.ModifiedByUser.DisplayName.Should().Be("Test User");
            footer.ModifiedByUser.Email.Should().Be("tester@example.com");
        }

        [Test]
        public void digital_object_identifier_can_be_saved_to_record()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            var record = BasicRecord().With(r => r.DigitalObjectIdentifier = "10.4124/1.ABC-123");
            service.Update(record, TestUser);

            Mock.Get(database).Verify(db => db.Store(record));
        }

        [Test]
        public void open_data_resource_paths_should_be_trimmed()
        {
            var database = Mock.Of<IDocumentSession>();
            var service = new RecordService(database, ValidatorStub());

            var record = BasicRecord().With(r => r.Publication = new PublicationInfo
            {
                OpenData = new OpenDataPublicationInfo
                {
                    Resources = new List<Resource>
                    {
                        new Resource { Path = "Z:\\does\\not\\need\\trimming.pdf" },
                        new Resource { Path = " Z:\\needs\\trimming\\left.pdf" },
                        new Resource { Path = "Z:\\needs\\trimming\\right.pdf " },
                        new Resource { Path = " Z:\\needs\\trimming\\both.pdf "} ,
                    }
                }
            });

            var expected = new List<string>
            {
                "Z:\\does\\not\\need\\trimming.pdf",
                "Z:\\needs\\trimming\\left.pdf",
                "Z:\\needs\\trimming\\right.pdf",
                "Z:\\needs\\trimming\\both.pdf",
            };

            var result = service.Update(record, TestUser);

            result.Record.Publication.OpenData.Resources.Select(r => r.Path).Should().ContainInOrder(expected);
        }

        Record BasicRecord()
        {
            return new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m => m.Title = "Some title"),
                Footer = new Footer()
            };
        }

        /// <summary>
        /// A validator stub which returns a result with no validation errors.
        /// </summary>
        IRecordValidator ValidatorStub()
        {
            return Mock.Of<IRecordValidator>(v => v.Validate(It.IsAny<Record>()) == new ValidationResult<Record>());
        }

        /// <summary>
        /// A validator stub which returns a result containing a validation error.
        /// </summary>
        IRecordValidator FailingValidatorStub()
        {
            var r = new ValidationResult<Record>();
            r.Errors.Add("Don't be silly!");
            return Mock.Of<IRecordValidator>(v => v.Validate(It.IsAny<Record>()) == r);
        }
    }
}
