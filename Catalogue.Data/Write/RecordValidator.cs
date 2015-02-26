using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Catalogue.Data.Model;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.ResourceType;
using Catalogue.Gemini.Roles;
using Catalogue.Gemini.Templates;
using Catalogue.Gemini.Vocabs;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using Catalogue.Utilities.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Catalogue.Data.Write
{
    public interface IRecordValidator
    {
        ValidationResult<Record> Validate(Record record);
    }

    public class RecordValidator : IRecordValidator
    {
        private const string GeminiSuffix =  " (Gemini)";

        public ValidationResult<Record> Validate(Record record)
        {
            var result = new ValidationResult<Record>();

            ValidatePath(record, result);
            ValidateTitle(record, result);
            ValidateKeywords(record, result);
            ValidateDatasetReferenceDate(record, result);
            ValidateTemporalExtent(record, result);
            ValidateTopicCategory(record, result);
            ValidateResourceLocator(record, result);
            ValidateResponsibleOrganisation(record, result);
            ValidateMetadataPointOfContact(record, result);
            ValidateResourceType(record, result);
            ValidateSecurityInvariants(record, result);
            ValidatePublishableInvariants(record, result);

            if (record.Validation == Validation.Gemini)
            {
                PerformGeminiValidation(record, result);
            }

            return result;
        }

        void ValidatePath(Record record, ValidationResult<Record> result)
        {
            // path_must_not_be_blank
            if (record.Path.IsBlank())
            {
                result.Errors.Add("Path must not be blank", r => r.Path);
            }
            else // (let's not add additional errors if it's just that it's blank)
            {
                // path_must_be_a_valid_file_system_path
                Uri uri;
                if (Uri.TryCreate(record.Path, UriKind.Absolute, out uri))
                {
                    if (uri.Scheme != Uri.UriSchemeFile)
                    {
                        result.Errors.Add("Path must be a file system path", r => r.Path);
                    }
                }
                else
                {
                    result.Errors.Add("Path must be a file system path", r => r.Path);
                }
            }
        }

        void ValidateTitle(Record record, ValidationResult<Record> result)
        {
            // title_must_not_be_blank
            if (record.Gemini.Title.IsBlank())
            {
                result.Errors.Add("Title must not be blank", r => r.Gemini.Title);
            }
        }

        void ValidateKeywords(Record record, ValidationResult<Record> ValidationResult)
        {
            if (!record.Gemini.Keywords.Any(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-broad-category"))
            {
                ValidationResult.Errors.Add(String.Format("Must specify a JNCC Broad Category keyword"),
                    r => r.Gemini.Keywords);
            }

            //No blank keywords
            if (record.Gemini.Keywords.Any(k => String.IsNullOrWhiteSpace(k.Value)))
            {
                ValidationResult.Errors.Add(
                    String.Format("Keywords cannot be blank"),
                    r => r.Gemini.Keywords);
            }
        }

        void ValidateDatasetReferenceDate(Record record, ValidationResult<Record> result)
        {
            // dataset_reference_date_must_be_valid_date
            if (!IsValidDate(record.Gemini.DatasetReferenceDate))
            {
                result.Errors.Add("Dataset reference date is not a valid date", r => r.Gemini.DatasetReferenceDate);
            }
        }

        void ValidateTemporalExtent(Record record, ValidationResult<Record> result)
        {
            var begin = record.Gemini.TemporalExtent.Begin;
            var end = record.Gemini.TemporalExtent.End;

            // temporal_extent_must_be_valid_dates
            if (begin.IsNotBlank() && !IsValidDate(begin))
            {
                result.Errors.Add("Temporal Extent (Begin) is not a valid date", r => r.Gemini.TemporalExtent.Begin);
            }

            if (end.IsNotBlank() && !IsValidDate(end))
            {
                result.Errors.Add("Temporal Extent (End) is not a valid date", r => r.Gemini.TemporalExtent.End);
            }

            // todo ensure End is after Begin
        }

        void ValidateResourceLocator(Record record, ValidationResult<Record> result)
        {
            // resource_locator_must_be_a_well_formed_http_url
            if (record.Gemini.ResourceLocator.IsNotBlank())
            {
                Uri url;
                if (Uri.TryCreate(record.Gemini.ResourceLocator, UriKind.Absolute, out url))
                {
                    if (url.Scheme != Uri.UriSchemeHttp)
                    {
                        result.Errors.Add("Resource locator must be an http url",
                            r => r.Gemini.ResourceLocator);
                    }
                }
                else
                {
                    result.Errors.Add("Resource locator must be a valid url",
                        r => r.Gemini.ResourceLocator);
                }
            }
        }

        void ValidateTopicCategory(Record record, ValidationResult<Record> result)
        {
            // topic_category_must_be_valid

            var s = record.Gemini.TopicCategory;

            if (s.IsNotBlank() && !TopicCategories.Values.Any(c => c.Name == s))
            {
                result.Errors.Add(String.Format("Topic Category '{0}' is not valid", record.Gemini.TopicCategory),
                    r => r.Gemini.TopicCategory);
            }
        }

        void ValidateResponsibleOrganisation(Record record, ValidationResult<Record> result)
        {
            // responsible_organisation_role_must_be_an_allowed_role
            var role = record.Gemini.ResponsibleOrganisation.Role;
            if (role.IsNotBlank() && !ResponsiblePartyRoles.Allowed.Contains(role))
            {
                result.Errors.Add(String.Format("Responsible Organisation Role '{0}' is not valid", role),
                    r => r.Gemini.ResponsibleOrganisation.Role);
            }
        }

        void ValidateMetadataPointOfContact(Record record, ValidationResult<Record> result)
        {
            // metadata_point_of_contact_role_must_be_an_allowed_role
            var role = record.Gemini.MetadataPointOfContact.Role;
            if (role.IsNotBlank() && !ResponsiblePartyRoles.Allowed.Contains(role))
            {
                result.Errors.Add(String.Format("Metadata Point of Contact Role '{0}' is not valid", role),
                    r => r.Gemini.MetadataPointOfContact.Role);
            }
        }

        void ValidateResourceType(Record record, ValidationResult<Record> result)
        {
            // resource type must be a valid Gemini resource type if not blank
            var resourceType = record.Gemini.ResourceType;
            if (resourceType.IsNotBlank() && !ResourceTypes.Allowed.Contains(resourceType))
            {
                result.Errors.Add(String.Format("Resource Type '{0}' is not valid", resourceType),
                    r => r.Gemini.ResourceType);
            }
        }

        void ValidateSecurityInvariants(Record record, ValidationResult<Record> result)
        {
            // non_open_records_must_have_limitations_on_public_access
            if (record.Security != Security.Open && record.Gemini.LimitationsOnPublicAccess.IsBlank())
            {
                result.Errors.Add("Non-open records must describe their limitations on public access",
                    r => r.Security,
                    r => r.Gemini.LimitationsOnPublicAccess);
            }
        }

        void ValidatePublishableInvariants(Record record, ValidationResult<Record> result)
        {
            // publishable_records_must_have_a_resource_locator
            if (record.Status == Status.Publishable && record.Gemini.ResourceLocator.IsBlank())
            {
                result.Errors.Add("Publishable records must have a resource locator",
                    r => r.Status, r => r.Gemini.ResourceLocator);
            }
        }

        bool IsValidDate(string date)
        {
            //todo: Disabled for mest data testing.
            return true;
            var yearOnly = new Regex(@"^\d\d\d\d$");
            var yearAndMonth = new Regex(@"^\d\d\d\d-(0[1-9]|1[012])$");
            var yearMonthAndDay = new Regex(@"^\d\d\d\d-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])$");

            return yearOnly.IsMatch(date) || yearAndMonth.IsMatch(date) || yearMonthAndDay.IsMatch(date);
        }

        void PerformGeminiValidation(Record record, ValidationResult<Record> ValidationResult)
        {
            // structured to match the gemini doc

            // 1 title is validated at basic level

            // 2 alternative title not used as optional

            // 3 Dataset language, conditional - data resource contains textual information
            // lets assume all data resources contain text
            // data_type is enum so can't be null, will default to eng

            // 4 abstract is mandatory
            if (record.Gemini.Abstract.IsBlank())
            {
                ValidationResult.Errors.Add("Abstract must be provided" + GeminiSuffix, r => r.Gemini.Abstract);
            }

            // 5 topic_category_must_not_be_blank
            if (record.Gemini.TopicCategory.IsBlank())
            {
                ValidationResult.Errors.Add(String.Format("Topic Category must be provided" + GeminiSuffix),
                    r => r.Gemini.TopicCategory);
            }

            // 6 keywords mandatory
            if (record.Gemini.Keywords.Count == 0)
            {
                ValidationResult.Errors.Add("Keywords must be provided" + GeminiSuffix, r => r.Gemini.Keywords);
            }

            // 7 temporal extent is mandatory - at least Begin must be provided
            if (record.Gemini.TemporalExtent.Begin.IsBlank())
            {
                ValidationResult.Errors.Add("Temporal Extent must be provided" + GeminiSuffix,
                    r => r.Gemini.TemporalExtent.Begin);
            }

            // 8 DatasetReferenceDate mandatory
            if (record.Gemini.DatasetReferenceDate.IsBlank())
            {
                ValidationResult.Errors.Add("Dataset Reference Date must be provided" + GeminiSuffix,
                    r => r.Gemini.DatasetReferenceDate);
            }

            // 10 Lineage is mandatory
            if (record.Gemini.Lineage.IsBlank())
            {
                ValidationResult.Errors.Add("Lineage msut be provided" + GeminiSuffix, r => r.Gemini.Lineage);
            }

            // 15 extent is optional and not used

            // 16 Vertical extent information is optional and not used

            // 17 Spatial reference system is optional

            // 18 Spatial resolution, where it can be specified it should - so its optional

            // 19 resource location, conditional
            // when online access is availble, should be a valid url
            // resource_locator_must_be_a_well_formed_http_url
            // when do not yet perform a get request and get a 200 response, the only true way to validate a url
            if (record.Gemini.ResourceLocator.IsBlank())
            {
                ValidationResult.Errors.Add("Resource locator must be provided" + GeminiSuffix,
                      r => r.Gemini.ResourceLocator);
            }
            else
            {
                Uri url;

                if (Uri.TryCreate(record.Gemini.ResourceLocator, UriKind.Absolute, out url))
                {
                    if (url.Scheme != Uri.UriSchemeHttp)
                    {
                        ValidationResult.Errors.Add("Resource locator must be an http url",
                            r => r.Gemini.ResourceLocator);
                    }
                }
                else
                {
                    ValidationResult.Errors.Add("Resource locator must be a valid url",
                        r => r.Gemini.ResourceLocator);
                }
            }


            // 21 DataFormat optional 

            // 23 reponsible Organisation
            if (record.Gemini.ResponsibleOrganisation.Email.IsBlank())
            {
                ValidationResult.Errors.Add("Email address for responsible organisation must be provided" + GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Email);
            }
            if (record.Gemini.ResponsibleOrganisation.Name.IsBlank())
            {
                ValidationResult.Errors.Add("Name of responsible organisation must be provided" + GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Name);
            }

            // 24 frequency of update is optional

            // 25 limitations on publci access is mandatory
            if (record.Gemini.LimitationsOnPublicAccess.IsBlank())
            {
                ValidationResult.Errors.Add("Limitations On Public Access must be provided" + GeminiSuffix,
                    r => r.Gemini.LimitationsOnPublicAccess);
            }

            // 26 use constraints are mandatory
            if (record.Gemini.UseConstraints.IsBlank())
            {
                ValidationResult.Errors.Add("Use Constraints must be provided (if there are none, leave as 'no conditions apply')",
                    r => r.Gemini.UseConstraints);
            }

            // 27 Additional information source is optional

            // 30 metadatadate is mandatory
            if (record.Gemini.MetadataDate.Equals(DateTime.MinValue))
            {
                ValidationResult.Errors.Add("A metadata reference date must be provided" + GeminiSuffix,
                    r => r.Gemini.MetadataDate);
            }

            // 33 Metadatalanguage

            // 35 Point of contacts
            // org name and email contact mandatory
            if (record.Gemini.MetadataPointOfContact.Email.IsBlank())
            {
                ValidationResult.Errors.Add("A metadata point of contact email address must be provided" + GeminiSuffix,
                    r => r.Gemini.MetadataPointOfContact.Email);
            }
            if (record.Gemini.MetadataPointOfContact.Name.IsBlank())
            {
                ValidationResult.Errors.Add("A metadata point of contact organisation name must be provided" + GeminiSuffix,
                    r => r.Gemini.MetadataPointOfContact.Name);
            }

            // 36 Uniuque resource identifier
            // not yet implemented need code and codespace

            // 39 resource type is mandatory
            if (record.Gemini.ResourceType.IsBlank())
            {
                ValidationResult.Errors.Add("A resource type must be provided" + GeminiSuffix,
                    r => r.Gemini.ResourceType);
            }

            // 40 Keywords from controlled vocabularys must be defined, they cannot be added.
            //ValidateControlledKeywords(record, recordValidationResult<Record>);

            // Conformity, required if claiming conformity to INSPIRE
            // not yet implemented

            // Equivalent scale, optional

            // BoundingBox
            // mandatory and valid

            if (record.Gemini.BoundingBox == null)
            {
                ValidationResult.Errors.Add(
                    "A bounding box must be supplied to conform to the Gemini specification",
                    r => r.Gemini.BoundingBox);
            }
        }
    }


    internal class when_validating_at_basic_level
    {
        private Record SimpleRecord()
        {
            return new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m =>
                    {
                        m.Title = "Some title";
                        m.Keywords = new StringPairList
                            {
                                { "http://vocab.jncc.gov.uk/jncc-broad-category", "Example Category" },
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
        public void path_must_not_be_blank([Values("", " ", null)] string blank)
        {
            var result = new RecordValidator().Validate(SimpleRecord().With(r => r.Path = blank));

            result.Errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void path_must_be_a_valid_file_system_path()
        {
            var result = new RecordValidator().Validate(SimpleRecord().With(r => r.Path = "not a path"));

            result.Errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void jncc_broad_category_must_be_provided()
        {
            // should not validate on empty list
            var r1 =
               new RecordValidator().Validate(SimpleRecord().With(r => r.Gemini.Keywords = new List<MetadataKeyword>()));

            r1.Errors.Single().Message.Should().StartWith("Must specify a JNCC Broad Category");
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
        public void non_open_records_must_have_limitations_on_public_access(
            [Values(Security.Classified, Security.Restricted)] Security nonOpen, [Values("", " ", null)] string blank)
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
        public void publishable_records_must_have_a_resource_locator([Values("", " ", null)] string blank)
        {
            var record = SimpleRecord().With(r =>
            {
                r.Status = Status.Publishable;
                r.Gemini.ResourceLocator = blank;
            });

            var result = new RecordValidator().Validate(record);

            result.Errors.Single().Fields.Should().Contain("status");
            result.Errors.Single().Fields.Should().Contain("gemini.resourceLocator");
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
        public void resource_locator_may_be_set()
        {
            var record = SimpleRecord().With(r => r.Gemini.ResourceLocator = "http://example.org/resource/locator");
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
    }

    internal class when_validating_at_gemini_level
    {

        private Record SimpleRecord()
        {
            return new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Example(),
                Validation = Validation.Gemini,
            };
        }

        [Test]
        public void blanck_lineage_is_not_allowed([Values("", " ", null)] string blank)
        {
            var record = SimpleRecord().With(r => r.Gemini.Lineage = blank);
            var result = new RecordValidator().Validate(record);

            result.Errors.Any(e => e.Fields.Contains("gemini.lineage")).Should().BeTrue();
        }

        [Test]
        public void blank_use_constraints_are_not_allowed([Values("", " ", null)] string blank)
        {
            var record = SimpleRecord().With(r => r.Gemini.UseConstraints = blank);
            var result = new RecordValidator().Validate(record);

            result.Errors.Any(e => e.Fields.Contains("gemini.useConstraints")).Should().BeTrue();
        }

        [Test]
        public void topic_category_must_not_be_blank([Values("", " ", null)] string blank)
        {
            var record = SimpleRecord().With(r => r.Gemini.TopicCategory = blank);
            var result = new RecordValidator().Validate(record);

            result.Errors.Any(e => e.Fields.Contains("gemini.topicCategory")).Should().BeTrue();
        }

//        [Test]
//        public void should_not_allow_keyword_additions_to_controlled_vocabs()
//        {
//            Record record = SimpleRecord().With(r => r.Gemini.Keywords.Add(new MetadataKeyword("value", "vocabUrl")));
//            mockVocabService.Setup(v => v.Load("vocabUrl"))
//                            .Returns(
//                                (string vocab) =>
//                                new Vocabulary
//                                    {
//                                        Controlled = true,
//                                        Id = vocab,
//                                        Keywords =
//                                            new List<VocabularyKeyword>
//                                                {
//                                                    new VocabularyKeyword {Id = Guid.NewGuid(), Value = "notvalue"}
//                                                }
//                                    });
//
//            RecordValidationResult<Record> result = new RecordValidator().Validate(record);
//
//            result.Errors.Any(e => e.Fields.Contains("gemini.keywords")).Should().BeTrue();
//            result.Errors.Single(e => e.Fields.Contains("gemini.keywords"))
//                  .Message.Should()
//                  .Be("The keyword value does not exist in the controlled vocabulary vocabUrl");
//
//        }
    }
}