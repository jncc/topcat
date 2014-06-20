using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.ResourceType;
using Catalogue.Gemini.Roles;
using Catalogue.Gemini.Templates;
using Catalogue.Gemini.Vocabs;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Expressions;
using Catalogue.Utilities.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Data.Write
{
    public interface IRecordValidator
    {
        RecordValidationResult Validate(Record record);
    }

    public class RecordValidator : IRecordValidator
    {
        private const string GeminiSuffix =  " (Gemini Compatibility).";

        public RecordValidationResult Validate(Record record)
        {
            var result = new RecordValidationResult();

            // path_must_not_be_blank
            if (record.Path.IsBlank())
            {
                result.Errors.Add("Location Path must not be blank", r => r.Path);
            }

            // also part of gemini spec
            // title_must_not_be_blank
            if (record.Gemini.Title.IsBlank())
            {
                result.Errors.Add("Title must not be blank", r => r.Gemini.Title);
            }

            // the below perform app specific checks, not gemini
            ValidateTopicCategory(record, result);
            ValidateResourceLocator(record, result);
            ValidateResponsibleOrganisation(record, result);
            ValidateMetadataPointOfContact(record, result);
            ValidateResourceType(record, result);

            // non_open_records_must_have_limitations_on_public_access
            if (record.Security != Security.Open && record.Gemini.LimitationsOnPublicAccess.IsBlank())
            {
                result.Errors.Add("Non-open records must describe their limitations on public access",
                    r => r.Security,
                    r => r.Gemini.LimitationsOnPublicAccess);
            }

            // publishable_records_must_have_a_resource_locator
            if (record.Status == Status.Publishable && record.Gemini.ResourceLocator.IsBlank())
            {
                result.Errors.Add("Publishable records must have a resource locator",
                    r => r.Status, r => r.Gemini.ResourceLocator);
            }
            if (record.Validation == Validation.Gemini)
            {
                GeminiValidation(record, result);
            }

            return result;
        }

        private void GeminiValidation(Record record, RecordValidationResult recordValidationResult)
        {
            // structured to match the gemini doc

            // 1 title, checked even if not gemini. But repeating here to be explicit and ease refactoring
            if (record.Gemini.Title.IsBlank())
            {
                recordValidationResult.Errors.Add("Title must be provided" + GeminiSuffix, r => r.Gemini.Title);
            }

            // 2 alternative title not used as optional

            // 3 Dataset language, conditional - data resource contains textual information
            // lets assume all data resources contain text
            // data_type is enum so can't be null, will default to eng - todo unit tests

            // 4 abstract is mandatory
            if (record.Gemini.Abstract.IsBlank())
            {
                recordValidationResult.Errors.Add("Abstract must be provided" , r => r.Gemini.Abstract);
            }

            // 5 topic_category_must_not_be_blank
            if (record.Gemini.TopicCategory.IsBlank())
            {
                recordValidationResult.Errors.Add(String.Format("Topic Category must be provided"+GeminiSuffix),
                    r => r.Gemini.TopicCategory);
            }

            // 6 keywords mandatory
            if (record.Gemini.Keywords.Count == 0)
            {
                recordValidationResult.Errors.Add("Keywords must be provided"+GeminiSuffix, r => r.Gemini.Keywords);
            }

            // 7 temporal extent is mandatory (so not DateTime.minvalue) and must be logical (they can be the same)
            if (record.Gemini.TemporalExtent.Begin > record.Gemini.TemporalExtent.End ||
                record.Gemini.TemporalExtent.Begin.Equals(DateTime.MinValue) ||
                record.Gemini.TemporalExtent.End.Equals(DateTime.MinValue))
            {
                recordValidationResult.Errors.Add("Temporal extent must be provided, and must begin before it ends"+GeminiSuffix,
                    r => r.Gemini.TemporalExtent);
            }

            // 8 DatasetReferenceDate mandatory
            if (record.Gemini.DatasetReferenceDate.Equals(DateTime.MinValue))
            {
                recordValidationResult.Errors.Add("Dataset Reference Date must be provided"+GeminiSuffix,
                    r => r.Gemini.DatasetReferenceDate);
            }

            // 10 Lineage is mandatory
            if (record.Gemini.Lineage.IsBlank())
            {
                recordValidationResult.Errors.Add("Lineage msut be provided"+GeminiSuffix, r => r.Gemini.TemporalExtent);
            }

            // 15 extent is optional and not used
            // deliberately not implmented

            // 16 Vertical extent information is optional and not used
            // deliberately not implmented

            // 17 Spatial reference system is optional
            // deliberately not implmented

            // 18 Spatial resolution, where it can be specified it should - so its optional
            // deliberately not implmented

            // 19 resource location, conditional
            // when online access is availble, should be a valid url
            // resource_locator_must_be_a_well_formed_http_url
            // when do not yet perform a get request and get a 200 response, the only true way to validate a url
            if (record.Gemini.ResourceLocator.IsBlank())
            {
                recordValidationResult.Errors.Add("Resource locator must be provided" + GeminiSuffix,
                      r => r.Gemini.ResourceLocator);
            }else{
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
            // deliberately not implmented

            // 23 reponsible Organisation
            if (record.Gemini.ResponsibleOrganisation.Email.IsBlank())
            {
                recordValidationResult.Errors.Add("Email address for responsible organisation must be provided"+GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Email);
            }
            if (record.Gemini.ResponsibleOrganisation.Name.IsBlank())
            {
                recordValidationResult.Errors.Add("Name of responsible organisation must be provided" + GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Name);
            }
            if (!record.Gemini.ResponsibleOrganisation.Role.Equals("distributor"))
            {
                recordValidationResult.Errors.Add("Distributor must be provided" + GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Role);
            }

            // 24 frequency of update is optional
            // deliberatley not implemented
             
            // 25 limitations on publci access is mandatory
            if (record.Gemini.LimitationsOnPublicAccess.IsBlank())
            {
                recordValidationResult.Errors.Add("Limitations On Public Access must be provided"+GeminiSuffix,
                    r => r.Gemini.LimitationsOnPublicAccess);
            }

            // 26 use constraints are mandatory
            if (record.Gemini.UseConstraints.IsBlank())
            {
                recordValidationResult.Errors.Add("Use Constraints must be provided if there are none, leave as 'no conditions apply'",
                    r => r.Gemini.UseConstraints);
            }

            // 27 Additional information source is optional
            // deliebrately not implemeted

            // 30 metadatadate is mandatory
            if (record.Gemini.MetadataDate.Equals(DateTime.MinValue))
            {
                recordValidationResult.Errors.Add("A metadata reference date must be provided"+GeminiSuffix,
                    r => r.Gemini.MetadataDate);
            }

            // 33 Metadatalanguage
            // deliberaretly not implemented, only supporting english

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
            

            // Conformity, required if claiming conformity to INSPIRE
            // not yet implemented

            // Equivalent scale, optional
            // deliberaty not implemented

            // BoundingBox
            // mandatory and valid

            if (record.Gemini.BoundingBox == null)
            {
                recordValidationResult.Errors.Add(
                    "A bounding box must be supplied to conform to the Gemini specification",
                    r => r.Gemini.BoundingBox);
            }
        }

        private void ValidateResourceLocator(Record record, RecordValidationResult result)
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

        private void ValidateTopicCategory(Record record, RecordValidationResult result)
        {
            // topic_category_must_be_valid

            string s = record.Gemini.TopicCategory;

            if (s.IsNotBlank() && !TopicCategories.Values.Keys.Any(k => k == s))
            {
                result.Errors.Add(String.Format("Topic Category '{0}' is not valid", record.Gemini.TopicCategory),
                    r => r.Gemini.TopicCategory);
            }
        }

        private void ValidateResponsibleOrganisation(Record record, RecordValidationResult result)
        {
            // responsible_organisation_role_must_be_an_allowed_role
            string role = record.Gemini.ResponsibleOrganisation.Role;
            if (role.IsNotBlank() && !ResponsiblePartyRoles.Allowed.Contains(role))
            {
                result.Errors.Add(String.Format("Responsible Organisation Role '{0}' is not valid", role),
                    r => r.Gemini.ResponsibleOrganisation.Role);
            }
        }

        private void ValidateMetadataPointOfContact(Record record, RecordValidationResult result)
        {
            // metadata_point_of_contact_role_must_be_an_allowed_role
            string role = record.Gemini.MetadataPointOfContact.Role;
            if (role.IsNotBlank() && !ResponsiblePartyRoles.Allowed.Contains(role))
            {
                result.Errors.Add(String.Format("Metadata Point of Contact Role '{0}' is not valid", role),
                    r => r.Gemini.MetadataPointOfContact.Role);
            }
        }

        private void ValidateResourceType(Record record, RecordValidationResult result)
        {
            // resource type must be a valid Gemini resource type if not blank
            string resourceType = record.Gemini.ResourceType;
            if (resourceType.IsNotBlank() && !ResourceTypes.Allowed.Contains(resourceType))
            {
                result.Errors.Add(String.Format("Resource Type '{0}' is not valid", resourceType),
                    r => r.Gemini.ResourceType);
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
        private Record SimpleRecord()
        {
            return new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank().With(m => m.Title = "Some title"),
            };
        }

        [Test]
        public void should_produce_no_warnings_by_default()
        {
            // the basic level of validation shouldn't produce warnings - that would be too annoying

            RecordValidationResult result = new RecordValidator().Validate(SimpleRecord() /* no Level argument */);
            result.Warnings.Should().BeEmpty();
        }

        [Test]
        public void title_must_not_be_blank([Values("", " ", null)] string blank)
        {
            RecordValidationResult result =
                new RecordValidator().Validate(SimpleRecord().With(r => r.Gemini.Title = blank));

            result.Errors.Single().Message.Should().StartWith("Title must not be blank");
            result.Errors.Single().Fields.Single().Should().Be("gemini.title");
        }

        [Test]
        public void path_must_not_be_blank([Values("", " ", null)] string blank)
        {
            RecordValidationResult result = new RecordValidator().Validate(SimpleRecord().With(r => r.Path = blank));

            result.Errors.Single().Message.Should().StartWith("Path must not be blank");
            result.Errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void topic_category_must_be_valid()
        {
            Record record = SimpleRecord().With(r => r.Gemini.TopicCategory = "anInvalidTopicCategory");
            RecordValidationResult result = new RecordValidator().Validate(record);

            result.Errors.Single().Message.Should().Contain("Topic Category 'anInvalidTopicCategory' is not valid");
            result.Errors.Single().Fields.Single().Should().Be("gemini.topicCategory");
        }

        [Test]
        public void topic_category_may_be_blank([Values("", null)] string blank)
        {
            Record record = SimpleRecord().With(r => r.Gemini.TopicCategory = blank);
            RecordValidationResult result = new RecordValidator().Validate(record);
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public void non_open_records_must_have_limitations_on_public_access(
            [Values(Security.Classified, Security.Restricted)] Security nonOpen, [Values("", " ", null)] string blank)
        {
            Record record = SimpleRecord().With(r =>
            {
                r.Security = nonOpen;
                r.Gemini.LimitationsOnPublicAccess = blank;
            });

            RecordValidationResult result = new RecordValidator().Validate(record);

            result.Errors.Single().Fields.Should().Contain("security");
            result.Errors.Single().Fields.Should().Contain("gemini.limitationsOnPublicAccess");
        }

        [Test]
        public void publishable_records_must_have_a_resource_locator([Values("", " ", null)] string blank)
        {
            Record record = SimpleRecord().With(r =>
            {
                r.Status = Status.Publishable;
                r.Gemini.ResourceLocator = blank;
            });

            RecordValidationResult result = new RecordValidator().Validate(record);

            result.Errors.Single().Fields.Should().Contain("status");
            result.Errors.Single().Fields.Should().Contain("gemini.resourceLocator");
        }

        [Test]
        public void resource_locator_must_be_a_well_formed_http_url(
            [Values(@"Z:\some\path", "utter rubbish")] string nonHttpUrl)
        {
            Record record = SimpleRecord().With(r => r.Gemini.ResourceLocator = nonHttpUrl);
            RecordValidationResult result = new RecordValidator().Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.resourceLocator");
        }

        [Test]
        public void resource_locator_may_be_set()
        {
            Record record = SimpleRecord().With(r => r.Gemini.ResourceLocator = "http://example.org/resource/locator");
            RecordValidationResult result = new RecordValidator().Validate(record);
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public void responsible_organisation_role_must_be_an_allowed_role()
        {
            Record record = SimpleRecord().With(r => r.Gemini.ResponsibleOrganisation = new ResponsibleParty
            {
                Email = "a.mann@example.com",
                Name = "A. Mann",
                Role = "some role that isn't allowed",
            });
            RecordValidationResult result = new RecordValidator().Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.responsibleOrganisation.role");
        }

        [Test]
        public void metadata_point_of_contact_role_must_be_an_allowed_role()
        {
            Record record = SimpleRecord().With(r => r.Gemini.MetadataPointOfContact = new ResponsibleParty
            {
                Email = "a.mann@example.com",
                Name = "A. Mann",
                Role = "some role that isn't allowed",
            });
            RecordValidationResult result = new RecordValidator().Validate(record);
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
        public void blank_use_constraints_could_be_a_mistake([Values("", " ", null)] string blank)
        {
            Record record = SimpleRecord().With(r => r.Gemini.UseConstraints = blank);
            RecordValidationResult result = new RecordValidator().Validate(record);

            result.Warnings.Single()
                .Message.Should()
                .Be("Use Constraints is empty; did you mean 'no conditions apply'?");
            result.Warnings.Single().Fields.Single().Should().Be("gemini.useConstraints");
        }

        [Test]
        public void topic_category_must_not_be_blank([Values("", " ", null)] string blank)
        {
            Record record = SimpleRecord().With(r => r.Gemini.TopicCategory = blank);
            RecordValidationResult result = new RecordValidator().Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.topicCategory");
        }
    }
}