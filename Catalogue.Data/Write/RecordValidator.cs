using Catalogue.Data.Model;
using Catalogue.Gemini.ResourceType;
using Catalogue.Gemini.Roles;
using Catalogue.Gemini.Spatial;
using Catalogue.Gemini.Vocabs;
using Catalogue.Utilities.Text;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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
            ValidateResponsibleOrganisation(record, result);
            ValidateMetadataPointOfContact(record, result);
            ValidateResourceType(record, result);
            ValidateSecurityInvariants(record, result);
            ValidateBoundingBox(record, result);
            ValidateJnccSpecificRules(record, result);
            ValidatePublishableResources(record, result);
            ValidateDoi(record, result);

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
                // path_must_be_an_acceptable_kind
                // currently, a file system path or paul's experimental OGR connection string
                Uri uri;

                // allow OGR connection strings (experimental)
                if (record.Path.StartsWith("PG:"))
                {
                    if (!Regex.IsMatch(record.Path, "PG:\".+\""))
                    {
                        result.Errors.Add("Path doesn't appear to be a valid OGR connection string");
                    }
                }
                else if (Uri.TryCreate(record.Path, UriKind.Absolute, out uri))
                {
                    if (uri.Scheme != Uri.UriSchemeFile && uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                    {
                        result.Errors.Add("Path must be a file system path or URL", r => r.Path);
                    }
                }
                else
                {
                    result.Errors.Add("Path is invalid. Normally should be a file system path or URL", r => r.Path);
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

            // title must be reasonable length
            if (record.Gemini.Title != null && (record.Gemini.Title.Length > 200))
            {
                if (record.Gemini.ResourceType == "publication" && record.Gemini.Title.Length > 250)
                {
                    result.Errors.Add("Title is too long. 250 characters or less, please", r => r.Gemini.Title);
                }
                else if (record.Gemini.ResourceType != "publication")
                {
                    result.Errors.Add("Title is too long. 200 characters or less, please", r => r.Gemini.Title);
                }
            }
        }

        void ValidateKeywords(Record record, ValidationResult<Record> ValidationResult)
        {
            var jnccDomainKeywords = record.Gemini.Keywords.Where(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-domain");
            var jnccCategoryKeywords = record.Gemini.Keywords.Where(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-category");

            if (!jnccDomainKeywords.Any())
            {
                ValidationResult.Errors.Add(String.Format("Must specify a JNCC Domain keyword"),
                    r => r.Gemini.Keywords);
            }
            if (!jnccCategoryKeywords.Any())
            {
                ValidationResult.Errors.Add(String.Format("Must specify a JNCC Category keyword"),
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
            if (!String.IsNullOrWhiteSpace(record.Gemini.DatasetReferenceDate) && !IsValidDate(record.Gemini.DatasetReferenceDate))
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
            // sensitive_records_must_have_limitations_on_public_access
            if (record.Security != Security.Official && record.Gemini.LimitationsOnPublicAccess.IsBlank())
            {
                result.Errors.Add("Non-open records must describe their limitations on public access",
                    r => r.Security,
                    r => r.Gemini.LimitationsOnPublicAccess);
            }
        }

        void ValidateBoundingBox(Record record, ValidationResult<Record> result)
        {
            if (!BoundingBoxUtility.IsBlank(record.Gemini.BoundingBox))
            {
                if (!BoundingBoxUtility.IsValid(record.Gemini.BoundingBox))
                    result.Errors.Add("Invalid bounding box", r => r.Gemini.BoundingBox);
            }
        }

        void ValidateJnccSpecificRules(Record record, ValidationResult<Record> result)
        {
            var meshGuiKeywords = record.Gemini.Keywords.Where(k => k.Vocab == "http://vocab.jncc.gov.uk/mesh-gui").ToList();

            if (meshGuiKeywords.Count > 1)
                result.Errors.Add("More than one MESH GUI keyword isn't allowed", r => r.Gemini.Keywords);

            if (meshGuiKeywords.Any(k => !Regex.IsMatch(k.Value, "^GB[0-9]{6}$")))
                result.Errors.Add("MESH GUI not valid", r => r.Gemini.Keywords);
        }

        void ValidateDoi(Record record, ValidationResult<Record> result)
        {
            var doi = record.DigitalObjectIdentifier;

            if (string.IsNullOrEmpty(doi)) return;

            // if not blank it should look like 10.4124/ABC-123 where the digits before the slash are
            // an account prefix and after the slash can be any combination of numbers, letters, - . _ : + and / 
            var regex = new Regex(@"^[0-9]{2}\.[0-9]+\/[a-zA-Z0-9\-\._\:\+\/]+$");
            if (!regex.Match(doi).Success)
            {
                result.Errors.Add("Digital Object Identifier is not in a valid format", r => r.DigitalObjectIdentifier);
            }
            else if (!record.Citation.IsNotBlank())
            {
                result.Errors.Add("Citation must be provided for DOI record", r => r.Citation);
            }
        }
        
        void ValidatePublishableResources(Record record, ValidationResult<Record> result)
        {
            var resources = record?.Publication?.Data?.Resources;

            if (resources != null)
            {
                var containsDuplicatePaths = false;
                var containsDuplicateNames = false;

                foreach (var resource in resources)
                {
                    if (resource.Path.IsBlank() || resource.Name.IsBlank())
                    {
                        result.Errors.Add("Publishable resource name and path must not be blank", r => r.Publication.Data.Resources);
                    }
                    else // (let's not add additional errors if it's just that it's blank)
                    {
                        if (Uri.TryCreate(resource.Path, UriKind.Absolute, out var uri))
                        {
                            if (uri.Scheme != Uri.UriSchemeFile && uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                            {
                                result.Errors.Add("Publishable resource path must be a file system path or URL", r => r.Publication.Data.Resources);
                            }
                        }
                        else
                        {
                            result.Errors.Add("Publishable resource path must be a file system path or URL", r => r.Publication.Data.Resources);
                        }
                    }

                    if (resources.Count(r => r.Path.Equals(resource.Path)) > 1)
                        containsDuplicatePaths = true;

                    if (resources.Count(r => r.Name.Equals(resource.Name)) > 1)
                        containsDuplicateNames = true;
                }

                if (containsDuplicatePaths)
                {
                    result.Errors.Add("Publishable resources must be unique - no duplicates", r => r.Publication.Data.Resources);
                }

                if (containsDuplicateNames)
                {
                    result.Errors.Add("Publishable resource names must be unique - no duplicates", r => r.Publication.Data.Resources);
                }
            }
        }

        public static bool IsValidDate(string date)
        {
            //todo: Disabled for mest data testing.
            var yearOnly = new Regex(@"^\d\d\d\d$");
            var yearAndMonth = new Regex(@"^\d\d\d\d-(0[1-9]|1[012])$");
            var yearMonthAndDay = new Regex(@"^\d\d\d\d-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])$");

            DateTime parsedDateTime;
            return yearOnly.IsMatch(date) || yearAndMonth.IsMatch(date) || (yearMonthAndDay.IsMatch(date) && DateTime.TryParse(date, out parsedDateTime))
            ;
        }

        void PerformGeminiValidation(Record record, ValidationResult<Record> result)
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
                result.Errors.Add("Abstract must be provided" + GeminiSuffix, r => r.Gemini.Abstract);
            }

            // 5 topic_category_must_not_be_blank
            if (record.Gemini.TopicCategory.IsBlank())
            {
                result.Errors.Add(String.Format("Topic Category must be provided" + GeminiSuffix),
                    r => r.Gemini.TopicCategory);
            }

            // 6 keywords mandatory
            if (record.Gemini.Keywords.Count == 0)
            {
                result.Errors.Add("Keywords must be provided" + GeminiSuffix, r => r.Gemini.Keywords);
            }

            // 7 temporal extent is mandatory for datasets - at least Begin must be provided
            if (record.Gemini.ResourceType == "dataset" && record.Gemini.TemporalExtent.Begin.IsBlank())
            {
                result.Errors.Add("Temporal Extent must be provided" + GeminiSuffix,
                    r => r.Gemini.TemporalExtent.Begin);
            }

            // 8 DatasetReferenceDate mandatory
            if (record.Gemini.DatasetReferenceDate.IsBlank())
            {
                result.Errors.Add("Dataset Reference Date must be provided" + GeminiSuffix,
                    r => r.Gemini.DatasetReferenceDate);
            }

            // 10 Lineage is mandatory
            if (record.Gemini.Lineage.IsBlank())
            {
                result.Errors.Add("Lineage must be provided" + GeminiSuffix, r => r.Gemini.Lineage);
            }

            // 15 extent is optional and not used

            // 16 Vertical extent information is optional and not used

            // 17 Spatial reference system is optional

            // 18 Spatial resolution, where it can be specified it should - so its optional

            // 19 resource location, conditional
            // already checked during basic validation

            // 21 DataFormat optional 

            // 23 reponsible Organisation
            if (record.Gemini.ResponsibleOrganisation.Email.IsBlank())
            {
                result.Errors.Add("Email address for responsible organisation must be provided" + GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Email);
            }
            if (record.Gemini.ResponsibleOrganisation.Name.IsBlank())
            {
                result.Errors.Add("Name of responsible organisation must be provided" + GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Name);
            }
            if (record.Gemini.ResponsibleOrganisation.Role.IsBlank())
            {
                result.Errors.Add("Role of responsible organisation must be provided" + GeminiSuffix,
                        r => r.Gemini.ResponsibleOrganisation.Role);
            }

            // 24 frequency of update is optional

            // 25 limitations on publci access is mandatory
            if (record.Gemini.LimitationsOnPublicAccess.IsBlank())
            {
                result.Errors.Add("Limitations On Public Access must be provided" + GeminiSuffix,
                    r => r.Gemini.LimitationsOnPublicAccess);
            }

            // 26 use constraints are mandatory
            if (record.Gemini.UseConstraints.IsBlank())
            {
                result.Errors.Add("Use Constraints must be provided (if there are none, leave as 'no conditions apply')",
                    r => r.Gemini.UseConstraints);
            }

            // 27 Additional information source is optional

            // 30 metadatadate is mandatory
            if (record.Gemini.MetadataDate.Equals(DateTime.MinValue))
            {
                result.Errors.Add("A metadata reference date must be provided" + GeminiSuffix,
                    r => r.Gemini.MetadataDate);
            }

            // 33 Metadatalanguage

            // 35 Point of contacts
            // org name and email contact mandatory
            if (record.Gemini.MetadataPointOfContact.Email.IsBlank())
            {
                result.Errors.Add("A metadata point of contact email address must be provided" + GeminiSuffix,
                    r => r.Gemini.MetadataPointOfContact.Email);
            }
            if (record.Gemini.MetadataPointOfContact.Name.IsBlank())
            {
                result.Errors.Add("A metadata point of contact organisation name must be provided" + GeminiSuffix,
                    r => r.Gemini.MetadataPointOfContact.Name);
            }
            if (record.Gemini.MetadataPointOfContact.Role != "pointOfContact")
            {
                result.Errors.Add("The metadata point of contact role must be 'pointOfContact'" + GeminiSuffix,
                    r => r.Gemini.MetadataPointOfContact.Name);
            }

            // 36 Unique resource identifier

            // 39 resource type is mandatory
            if (record.Gemini.ResourceType.IsBlank())
            {
                result.Errors.Add("A resource type must be provided" + GeminiSuffix, 
                    r => r.Gemini.ResourceType);
            }

            // 40 Keywords from controlled vocabularys must be defined, they cannot be added.
            //ValidateControlledKeywords(record, recordValidationResult<Record>);

            // Conformity, required if claiming conformity to INSPIRE
            // not yet implemented

            // Equivalent scale, optional

            // we're going to try to squash gemini and non-geographic iso metadata together in the same validation
            if (record.Gemini.ResourceType == "dataset")
            {
                // BoundingBox
                // mandatory
                // valid todo

                if (BoundingBoxUtility.IsBlank(record.Gemini.BoundingBox))
                {
                    result.Errors.Add("A bounding box must be supplied", r => r.Gemini.BoundingBox);
                }
            }
        }
    }
}