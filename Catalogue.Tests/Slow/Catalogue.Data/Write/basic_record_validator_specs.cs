using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Catalogue.Tests.Slow.Catalogue.Data.Write
{
    class basic_record_validator_specs
    {
        Record SimpleRecord()
        {
            return new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Some title";
                    m.Keywords = new StringPairList
                        {
                            { "http://vocab.jncc.gov.uk/jncc-domain", "Example Domain" },
                            { "http://vocab.jncc.gov.uk/jncc-category", "Example Category" },
                        }
                        .ToKeywordList();
                }),
            };
        }

        [Test]
        public void should_produce_no_warnings_by_default()
        {
            // the basic level of validation shouldn't produce warnings - that would be too annoying

            var result = new RecordValidator().Validate(SimpleRecord() /* no Level argument */);
            result.Warnings.Should().BeEmpty();
        }

        [Test]
        public void title_must_not_be_blank([Values("", " ", null)] string blank)
        {
            var result =
                new RecordValidator().Validate(SimpleRecord().With(r => r.Gemini.Title = blank));

            result.Errors.Single().Message.Should().StartWith("Title must not be blank");
            result.Errors.Single().Fields.Single().Should().Be("gemini.title");
        }

        [Test]
        public void title_must_not_be_longer_then_200([Values("", " ", null)] string blank)
        {
            var result =
                new RecordValidator().Validate(SimpleRecord().With(r => r.Gemini.Title = new String('x', 201)));

            result.Errors.Single().Message.Should().StartWith("Title is too long. 200 characters or less, please");
            result.Errors.Single().Fields.Single().Should().Be("gemini.title");
        }

        [Test]
        public void publication_title_must_not_be_longer_then_250([Values("", " ", null)] string blank)
        {
            var result =
                new RecordValidator().Validate(SimpleRecord().With(r => {
                    r.Gemini.Title = new String('x', 251);
                    r.Gemini.ResourceType = "publication";
                }));

            result.Errors.Single().Message.Should().StartWith("Title is too long. 250 characters or less, please");
            result.Errors.Single().Fields.Single().Should().Be("gemini.title");
        }

        [Test]
        public void publication_title_can_be_longer_then_150([Values("", " ", null)] string blank)
        {
            var result =
                new RecordValidator().Validate(SimpleRecord().With(r =>
                {
                    r.Gemini.Title = new String('x', 155);
                    r.Gemini.ResourceType = "publication";
                }));

            result.Errors.Count().Should().Be(0);
        }

        [Test]
        public void path_must_not_be_blank([Values("", " ", null)] string blank)
        {
            var result = new RecordValidator().Validate(SimpleRecord().With(r => r.Path = blank));

            result.Errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void path_must_be_valid()
        {
            var result = new RecordValidator().Validate(SimpleRecord().With(r => r.Path = "not a valid path"));
            result.Errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void path_must_be_an_acceptable_kind([Values(
            @"X:\some\path",
            "PG:\"host=spatial-store dbname=spatial layer=SSSI_England_Units\"",
            @"\\jncc-corpfile\jncc corporate data\my_dataset.xlsx",
            @"http://www.example.com",
            @"https://www.example.com",
            @"postgres://username@hostname/databasename")] string path)
        {
            var result = new RecordValidator().Validate(SimpleRecord().With(r => r.Path = path));
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public void jncc_keywords_must_be_provided()
        {
            // should not validate on empty list
            var r1 =
                new RecordValidator().Validate(SimpleRecord().With(r => r.Gemini.Keywords = new List<MetadataKeyword>()));

            r1.Errors.First().Message.Should().StartWith("Must specify a JNCC Domain");
        }

        [Test]
        public void keywords_may_not_be_blank()
        {
            var record = SimpleRecord();
            record.Gemini.Keywords.Add(new MetadataKeyword() { Value = String.Empty, Vocab = String.Empty });

            var result =
                new RecordValidator().Validate(record);

            result.Errors.Single().Message.Should().StartWith("Keywords cannot be blank");
            result.Errors.Single().Fields.Single().Should().Be("gemini.keywords");
        }

        [Test]
        public void dataset_reference_date_must_be_valid_date([Values("", " ", null)] string invalid)
        {
            var validDates = new[]
            {
                "",
                "",
                "",
                "",
            };
            //            var result = new RecordValidator().Validate(SimpleRecord().With(r => r.Gemini.DatasetReferenceDate));
            //
            //            result.Errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void topic_category_must_be_valid()
        {
            var record = SimpleRecord().With(r => r.Gemini.TopicCategory = "anInvalidTopicCategory");
            var result = new RecordValidator().Validate(record);

            result.Errors.Single().Message.Should().Contain("Topic Category 'anInvalidTopicCategory' is not valid");
            result.Errors.Single().Fields.Single().Should().Be("gemini.topicCategory");
        }

        [Test]
        public void topic_category_may_be_blank([Values("", null)] string blank)
        {
            var record = SimpleRecord().With(r => r.Gemini.TopicCategory = blank);
            var result = new RecordValidator().Validate(record);
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public void sensitive_records_must_have_limitations_on_public_access(
            [Values(Security.Secret, Security.OfficialSensitive)] global::Catalogue.Data.Model.Security nonOpen, [Values("", " ", null)] string blank)
        {
            var record = SimpleRecord().With(r =>
            {
                r.Security = nonOpen;
                r.Gemini.LimitationsOnPublicAccess = blank;
            });

            var result = new RecordValidator().Validate(record);

            result.Errors.Single().Fields.Should().Contain("security");
            result.Errors.Single().Fields.Should().Contain("gemini.limitationsOnPublicAccess");
        }

        [Test]
        public void resource_locator_must_be_a_well_formed_http_url(
            [Values(@"Z:\some\path", "utter rubbish")] string nonHttpUrl)
        {
            var record = SimpleRecord().With(r => r.Gemini.ResourceLocator = nonHttpUrl);
            var result = new RecordValidator().Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.resourceLocator");
        }

        [Test]
        public void resource_locator_may_be_set(
            [Values(@"http://www.example.org/resource/locator", @"https://www.example.org/resource/locator")] string httpUrl)
        {
            var record = SimpleRecord().With(r => r.Gemini.ResourceLocator = httpUrl);
            var result = new RecordValidator().Validate(record);
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public void responsible_organisation_role_must_be_an_allowed_role()
        {
            var record = SimpleRecord().With(r => r.Gemini.ResponsibleOrganisation = new ResponsibleParty
            {
                Email = "a.mann@example.com",
                Name = "A. Mann",
                Role = "some role that isn't allowed",
            });
            var result = new RecordValidator().Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.responsibleOrganisation.role");
        }

        [Test]
        public void metadata_point_of_contact_role_must_be_an_allowed_role()
        {
            var record = SimpleRecord().With(r => r.Gemini.MetadataPointOfContact = new ResponsibleParty
            {
                Email = "a.mann@example.com",
                Name = "A. Mann",
                Role = "some role that isn't allowed",
            });
            var result = new RecordValidator().Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.metadataPointOfContact.role");
        }

        [Test]
        public void should_validate_valid_dates()
        {
            RecordValidator.IsValidDate("2017").Should().BeTrue();
            RecordValidator.IsValidDate("2017-01").Should().BeTrue();
            RecordValidator.IsValidDate("2017-01-01").Should().BeTrue();
            RecordValidator.IsValidDate("2012-02-29").Should().BeTrue(); //leap year

        }

        [Test]
        public void should_not_validate_invalid_dates()
        {
            RecordValidator.IsValidDate("aaa").Should().BeFalse();
            RecordValidator.IsValidDate("August 2009").Should().BeFalse();
            RecordValidator.IsValidDate("2013-02-29").Should().BeFalse(); //not a leap year
        }

        [Test]
        public void mesh_gui_keywords_must_be_of_valid_form()
        {
            var record = SimpleRecord().With(r => r.Gemini.Keywords.Add(new MetadataKeyword { Vocab = "http://vocab.jncc.gov.uk/mesh-gui", Value = "blah" }));
            var result = new RecordValidator().Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.keywords");
        }

        [Test]
        public void doi_with_invalid_formats([Values(" ", "a bad doi", "baddoi", "104124/ABC-123", "10.4124/ABC-123?", "AB.1234/ABC-123")] string doi)
        {
            var record = SimpleRecord().With(r => r.DigitalObjectIdentifier = doi);
            var result = new RecordValidator().Validate(record);

            result.Errors.Single().Message.Should().Contain("Digital Object Identifier is not in a valid format");
            result.Errors.Single().Fields.Single().Should().Be("digitalObjectIdentifier");
        }

        [Test]
        public void doi_with_valid_formats([Values(null, "" ,"12.3456/ABC123", "00.1234/long.string+which-is:still_valid/123")] string doi)
        {
            var record = SimpleRecord().With(r => r.DigitalObjectIdentifier = doi);
            var result = new RecordValidator().Validate(record);

            result.Errors.Should().BeEmpty();
        }
    }
}
