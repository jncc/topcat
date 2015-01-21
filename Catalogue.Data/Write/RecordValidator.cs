using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Catalogue.Data.Model;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.ResourceType;
using Catalogue.Gemini.Roles;
using Catalogue.Gemini.Templates;
using Catalogue.Gemini.Vocabs;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using Catalogue.Utilities.Expressions;
using Catalogue.Utilities.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Catalogue.Data.Write
{
    public interface IRecordValidator
    {
        RecordValidationResult Validate(Record record);
    }

    public class RecordValidator : IRecordValidator
    {
        private const string GeminiSuffix =  " (Gemini)";

        private readonly IVocabularyService vocabService;

        public RecordValidator(IVocabularyService vocabService)
        {
            this.vocabService = vocabService;
        }

        public RecordValidationResult Validate(Record record)
        {
            var result = new RecordValidationResult();

            ValidatePath(record, result);
            ValidateTitle(record, result);
            ValidateKeywords(record, result);
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

        void ValidatePath(Record record, RecordValidationResult result)
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

        void ValidateTitle(Record record, RecordValidationResult result)
        {
            // title_must_not_be_blank
            if (record.Gemini.Title.IsBlank())
            {
                result.Errors.Add("Title must not be blank", r => r.Gemini.Title);
            }
        }

        void ValidateKeywords(Record record, RecordValidationResult recordValidationResult)
        {
            //Must be one, non blank keyword

            if (record.Gemini.Keywords.All(k => String.IsNullOrWhiteSpace(k.Value)))
            {
                recordValidationResult.Errors.Add(String.Format("At least one keyword must be specified" + GeminiSuffix),
                    r => r.Gemini.Keywords);
            }

            //No blank keywords
            if (record.Gemini.Keywords.Any(k => String.IsNullOrWhiteSpace(k.Value)))
            {
                recordValidationResult.Errors.Add(
                    String.Format("Keywords cannot be blank" + GeminiSuffix),
                    r => r.Gemini.Keywords);
            }


        }

        void ValidateDatasetReferenceDate(Record record, RecordValidationResult result)
        {
            // dataset_reference_date_must_be_valid_date
            if (IsValidDate(record.Gemini.DatasetReferenceDate))
            {
                result.Errors.Add("Dataset reference date is not a valid date", r => r.Gemini.DatasetReferenceDate);
            }
        }

        void ValidateResourceLocator(Record record, RecordValidationResult result)
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

        void ValidateTopicCategory(Record record, RecordValidationResult result)
        {
            // topic_category_must_be_valid

            var s = record.Gemini.TopicCategory;

            if (s.IsNotBlank() && !TopicCategories.Values.Any(c => c.Name == s))
            {
                result.Errors.Add(String.Format("Topic Category '{0}' is not valid", record.Gemini.TopicCategory),
                    r => r.Gemini.TopicCategory);
            }
        }

        void ValidateResponsibleOrganisation(Record record, RecordValidationResult result)
        {
            // responsible_organisation_role_must_be_an_allowed_role
            var role = record.Gemini.ResponsibleOrganisation.Role;
            if (role.IsNotBlank() && !ResponsiblePartyRoles.Allowed.Contains(role))
            {
                result.Errors.Add(String.Format("Responsible Organisation Role '{0}' is not valid", role),
                    r => r.Gemini.ResponsibleOrganisation.Role);
            }
        }

        void ValidateMetadataPointOfContact(Record record, RecordValidationResult result)
        {
            // metadata_point_of_contact_role_must_be_an_allowed_role
            var role = record.Gemini.MetadataPointOfContact.Role;
            if (role.IsNotBlank() && !ResponsiblePartyRoles.Allowed.Contains(role))
            {
                result.Errors.Add(String.Format("Metadata Point of Contact Role '{0}' is not valid", role),
                    r => r.Gemini.MetadataPointOfContact.Role);
            }
        }

        void ValidateResourceType(Record record, RecordValidationResult result)
        {
            // resource type must be a valid Gemini resource type if not blank
            var resourceType = record.Gemini.ResourceType;
            if (resourceType.IsNotBlank() && !ResourceTypes.Allowed.Contains(resourceType))
            {
                result.Errors.Add(String.Format("Resource Type '{0}' is not valid", resourceType),
                    r => r.Gemini.ResourceType);
            }
        }

        void ValidateSecurityInvariants(Record record, RecordValidationResult result)
        {
            // non_open_records_must_have_limitations_on_public_access
            if (record.Security != Security.Open && record.Gemini.LimitationsOnPublicAccess.IsBlank())
            {
                result.Errors.Add("Non-open records must describe their limitations on public access",
                    r => r.Security,
                    r => r.Gemini.LimitationsOnPublicAccess);
            }
        }

        void ValidatePublishableInvariants(Record record, RecordValidationResult result)
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
            return true; // todo
        }

        void PerformGeminiValidation(Record record, RecordValidationResult recordValidationResult)
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
                recordValidationResult.Errors.Add("Abstract must be provided" + GeminiSuffix, r => r.Gemini.Abstract);
            }

            // 5 topic_category_must_not_be_blank
            if (record.Gemini.TopicCategory.IsBlank())
            {
                recordValidationResult.Errors.Add(String.Format("Topic Category must be provided" + GeminiSuffix),
                    r => r.Gemini.TopicCategory);
            }

            // 6 keywords mandatory
            if (record.Gemini.Keywords.Count == 0)
            {
                recordValidationResult.Errors.Add("Keywords must be provided" + GeminiSuffix, r => r.Gemini.Keywords);
            }

            // 7 temporal extent is mandatory (so not DateTime.minvalue) and must be logical (they can be the same)
            if (record.Gemini.TemporalExtent.Begin > record.Gemini.TemporalExtent.End ||
                record.Gemini.TemporalExtent.Begin.Equals(DateTime.MinValue) ||
                record.Gemini.TemporalExtent.End.Equals(DateTime.MinValue))
            {
                recordValidationResult.Errors.Add("Temporal extent must be provided, and must begin before it ends" + GeminiSuffix,
                    r => r.Gemini.TemporalExtent);
            }

            // 8 DatasetReferenceDate mandatory
            if (record.Gemini.DatasetReferenceDate.IsBlank())
            {
                recordValidationResult.Errors.Add("Dataset Reference Date must be provided" + GeminiSuffix,
                    r => r.Gemini.DatasetReferenceDate);
            }

            // 10 Lineage is mandatory
            if (record.Gemini.Lineage.IsBlank())
            {
                recordValidationResult.Errors.Add("Lineage msut be provided" + GeminiSuffix, r => r.Gemini.TemporalExtent);
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
                recordValidationResult.Errors.Add("Resource locator must be provided" + GeminiSuffix,
                      r => r.Gemini.ResourceLocator);
            }
            else
            {
                Uri url;

                if (Uri.TryCreate(record.Gemini.ResourceLocator, UriKind.Absolute, out url))
                {
                    if (url.Scheme != Uri.UriSchemeHttp)
                    {
                        recordValidationResult.Errors.Add("Resource locator must be an http url",
                            r => r.Gemini.ResourceLocator);
                    }
                }
                else
                {
                    recordValidationResult.Errors.Add("Resource locator must be a valid url",
                        r => r.Gemini.ResourceLocator);
                }
            }


            // 21 DataFormat optional 

            // 23 reponsible Organisation
            if (record.Gemini.ResponsibleOrganisation.Email.IsBlank())
            {
                recordValidationResult.Errors.Add("Email address for responsible organisation must be provided" + GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Email);
            }
            if (record.Gemini.ResponsibleOrganisation.Name.IsBlank())
            {
                recordValidationResult.Errors.Add("Name of responsible organisation must be provided" + GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Name);
            }

            // 24 frequency of update is optional

            // 25 limitations on publci access is mandatory
            if (record.Gemini.LimitationsOnPublicAccess.IsBlank())
            {
                recordValidationResult.Errors.Add("Limitations On Public Access must be provided" + GeminiSuffix,
                    r => r.Gemini.LimitationsOnPublicAccess);
            }

            // 26 use constraints are mandatory
            if (record.Gemini.UseConstraints.IsBlank())
            {
                recordValidationResult.Errors.Add("Use Constraints must be provided (if there are none, leave as 'no conditions apply')",
                    r => r.Gemini.UseConstraints);
            }

            // 27 Additional information source is optional

            // 30 metadatadate is mandatory
            if (record.Gemini.MetadataDate.Equals(DateTime.MinValue))
            {
                recordValidationResult.Errors.Add("A metadata reference date must be provided" + GeminiSuffix,
                    r => r.Gemini.MetadataDate);
            }

            // 33 Metadatalanguage

            // 35 Point of contacts
            // org name and email contact mandatory
            if (record.Gemini.MetadataPointOfContact.Email.IsBlank())
            {
                recordValidationResult.Errors.Add("A metadata point of contact email address must be provided" + GeminiSuffix,
                    r => r.Gemini.MetadataPointOfContact.Email);
            }
            if (record.Gemini.MetadataPointOfContact.Name.IsBlank())
            {
                recordValidationResult.Errors.Add("A metadata point of contact organisation name must be provided" + GeminiSuffix,
                    r => r.Gemini.MetadataPointOfContact.Name);
            }

            // 36 Uniuque resource identifier
            // not yet implemented need code and codespace

            // 39 resource type is mandatory
            if (record.Gemini.ResourceType.IsBlank())
            {
                recordValidationResult.Errors.Add("A resource type must be provided" + GeminiSuffix,
                    r => r.Gemini.ResourceType);
            }

            // 40 Keywords from controlled vocabularys must be defined, they cannot be added.
            //ValidateControlledKeywords(record, recordValidationResult);

            // Conformity, required if claiming conformity to INSPIRE
            // not yet implemented

            // Equivalent scale, optional

            // BoundingBox
            // mandatory and valid

            if (record.Gemini.BoundingBox == null)
            {
                recordValidationResult.Errors.Add(
                    "A bounding box must be supplied to conform to the Gemini specification",
                    r => r.Gemini.BoundingBox);
            }
        }
    }

    public class RecordValidationIssue
    {
        public RecordValidationIssue(string message, List<Expression<Func<Record, object>>> fields)
        {
            Message = message;
            FieldExpressions = fields;
        }

        public string Message { get; private set; }

        private List<Expression<Func<Record, object>>> FieldExpressions { get; set; }

        /// <summary>
        ///     A representation of the property accessor expression(s) suitable for eg a json client.
        /// </summary>
        public List<string> Fields
        {
            get
            {
                return (from e in FieldExpressions
                    let expression = e.Body.RemoveUnary()
                    let fullDottedPath = expression.ToString().Replace("r.", String.Empty)
                    let camelCasedProperties = fullDottedPath.Split('.').Select(StringUtility.ToCamelCase)
                    select String.Join(".", camelCasedProperties))
                    .ToList();
            }
        }
    }

    public class RecordValidationIssueSet : Collection<RecordValidationIssue>
    {
        public void Add(string message, params Expression<Func<Record, object>>[] fields)
        {
            Add(new RecordValidationIssue(message, fields.ToList()));
        }

        public void Append(RecordValidationIssueSet source)
        {
            foreach (var issue in source)
            {
                Add(issue);
            }
        }
    }

    public class RecordValidationResult
    {
        public RecordValidationResult()
        {
            Errors = new RecordValidationIssueSet();
            Warnings = new RecordValidationIssueSet();
        }

        public RecordValidationIssueSet Errors { get; private set; }
        public RecordValidationIssueSet Warnings { get; private set; }
    }


    internal class when_validating_at_basic_level
    {
        private Mock<IVocabularyService> mockVocabService = new Mock<IVocabularyService>();

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

            var result = new RecordValidator(mockVocabService.Object).Validate(SimpleRecord() /* no Level argument */);
            result.Warnings.Should().BeEmpty();
        }

        [Test]
        public void title_must_not_be_blank([Values("", " ", null)] string blank)
        {
            var result =
                new RecordValidator(mockVocabService.Object).Validate(SimpleRecord().With(r => r.Gemini.Title = blank));

            result.Errors.Single().Message.Should().StartWith("Title must not be blank");
            result.Errors.Single().Fields.Single().Should().Be("gemini.title");
        }

        [Test]
        public void path_must_not_be_blank([Values("", " ", null)] string blank)
        {
            var result = new RecordValidator(mockVocabService.Object).Validate(SimpleRecord().With(r => r.Path = blank));

            result.Errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void path_must_be_a_valid_file_system_path()
        {
            var result = new RecordValidator(mockVocabService.Object).Validate(SimpleRecord().With(r => r.Path = "not a path"));

            result.Errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void one_non_blank_keyword_must_be_provided()
        {
            // should not validate on empty list
            var r1 =
               new RecordValidator(mockVocabService.Object).Validate(SimpleRecord().With(r => r.Gemini.Keywords = new List<MetadataKeyword>()));

            r1.Errors.Single().Message.Should().StartWith("At least one keyword must be specified");
            r1.Errors.Single().Fields.Single().Should().Be("gemini.keywords");

            //should not validate on list with blank keywords
            var r2 = new RecordValidator(mockVocabService.Object).Validate(SimpleRecord().With(r => r.Gemini.Keywords = new StringPairList
                        {
                            {"", ""},
                        }
                        .ToKeywordList()));


            r2.Errors.First().Message.Should().StartWith("At least one keyword must be specified");
            r2.Errors.First().Fields.Single().Should().Be("gemini.keywords");

        }

        [Test]
        public void keywords_may_not_be_blank()
        {
            var record = SimpleRecord();
            record.Gemini.Keywords.Add(new MetadataKeyword() { Value = String.Empty, Vocab = String.Empty });

            var result =
                new RecordValidator(mockVocabService.Object).Validate(record);

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
//            var result = new RecordValidator(mockVocabService.Object).Validate(SimpleRecord().With(r => r.Gemini.DatasetReferenceDate));
//
//            result.Errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void topic_category_must_be_valid()
        {
            var record = SimpleRecord().With(r => r.Gemini.TopicCategory = "anInvalidTopicCategory");
            var result = new RecordValidator(mockVocabService.Object).Validate(record);

            result.Errors.Single().Message.Should().Contain("Topic Category 'anInvalidTopicCategory' is not valid");
            result.Errors.Single().Fields.Single().Should().Be("gemini.topicCategory");
        }

        [Test]
        public void topic_category_may_be_blank([Values("", null)] string blank)
        {
            var record = SimpleRecord().With(r => r.Gemini.TopicCategory = blank);
            var result = new RecordValidator(mockVocabService.Object).Validate(record);
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

            var result = new RecordValidator(mockVocabService.Object).Validate(record);

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

            var result = new RecordValidator(mockVocabService.Object).Validate(record);

            result.Errors.Single().Fields.Should().Contain("status");
            result.Errors.Single().Fields.Should().Contain("gemini.resourceLocator");
        }

        [Test]
        public void resource_locator_must_be_a_well_formed_http_url(
            [Values(@"Z:\some\path", "utter rubbish")] string nonHttpUrl)
        {
            var record = SimpleRecord().With(r => r.Gemini.ResourceLocator = nonHttpUrl);
            var result = new RecordValidator(mockVocabService.Object).Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.resourceLocator");
        }

        [Test]
        public void resource_locator_may_be_set()
        {
            var record = SimpleRecord().With(r => r.Gemini.ResourceLocator = "http://example.org/resource/locator");
            var result = new RecordValidator(mockVocabService.Object).Validate(record);
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
            var result = new RecordValidator(mockVocabService.Object).Validate(record);
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
            var result = new RecordValidator(mockVocabService.Object).Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.metadataPointOfContact.role");
        }
    }

    internal class when_validating_at_gemini_level
    {
        private Mock<IVocabularyService> mockVocabService = new Mock<IVocabularyService>();

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
        public void blank_use_constraints_are_not_allowed([Values("", " ", null)] string blank)
        {
            var record = SimpleRecord().With(r => r.Gemini.UseConstraints = blank);
            var result = new RecordValidator(mockVocabService.Object).Validate(record);

            result.Errors.Any(e => e.Fields.Contains("gemini.useConstraints")).Should().BeTrue();
        }

        [Test]
        public void topic_category_must_not_be_blank([Values("", " ", null)] string blank)
        {
            var record = SimpleRecord().With(r => r.Gemini.TopicCategory = blank);
            var result = new RecordValidator(mockVocabService.Object).Validate(record);

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
//            RecordValidationResult result = new RecordValidator(mockVocabService.Object).Validate(record);
//
//            result.Errors.Any(e => e.Fields.Contains("gemini.keywords")).Should().BeTrue();
//            result.Errors.Single(e => e.Fields.Contains("gemini.keywords"))
//                  .Message.Should()
//                  .Be("The keyword value does not exist in the controlled vocabulary vocabUrl");
//
//        }
    }
}