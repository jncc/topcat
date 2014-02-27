using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
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
        RecordValidationErrorSet Validate(Record record);
    }

    public class RecordValidator : IRecordValidator
    {
        public RecordValidationErrorSet Validate(Record record)
        {
            var results = new RecordValidationErrorSet();

            // path_must_not_be_blank
            if (record.Path.IsBlank())
                results.Add("Path must not be blank.", r => r.Path);

            // topic_category_must_be_valid
            if (!IsTopicCategoryValid(record.Gemini.TopicCategory))
                results.Add(
                    String.Format("Topic Category '{0}' is not valid.", record.Gemini.TopicCategory),
                    r => r.Gemini.TopicCategory);

            // non_open_records_must_describe_their_limitations_on_public_access
            if (record.Security != Security.Open && record.Gemini.LimitationsOnPublicAccess.IsBlank())
                results.Add(
                    String.Format("Non-open records must describe their limitations on public access"),
                    r => r.Security, r => r.Gemini.LimitationsOnPublicAccess);

            return results;
        }

        static bool IsTopicCategoryValid(string s)
        {
            return s.IsBlank() || TopicCategories.Values.Keys.Any(k => k == s);
        }
    }


    public class RecordValidationError
    {
        public RecordValidationError(string message, List<Expression<Func<Record, object>>> fields)
        {
            Message = message;
            FieldExpressions = fields;
        }

        public string Message { get; private set; }
        List<Expression<Func<Record, object>>> FieldExpressions { get; set; }

        /// <summary>
        /// A representation of the property accessor expression(s) suitable for eg a json client.
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

    public class RecordValidationErrorSet : Collection<RecordValidationError>
    {
        public void Add(string message, params Expression<Func<Record, object>>[] fields)
        {
            this.Add(new RecordValidationError(message, fields.ToList()));
        }
    }

    class validator_specs
    {
        Record BlankRecord()
        {
            return new Record { Path = @"X:\some\path", Gemini = Library.Blank() };
        }

        [Test]
        public void path_must_not_be_blank([Values("", " ", null)] string path)
        {
            var errors = new RecordValidator().Validate(BlankRecord().With(r => r.Path = path));

            errors.Single().Message.Should().StartWith("Path must not be blank");
            errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void topic_category_must_be_valid()
        {
            var record = BlankRecord().With(r => r.Gemini.TopicCategory = "anInvalidTopicCategory");
            var errors = new RecordValidator().Validate(record);

            errors.Single().Message.Should().Contain("Topic Category 'anInvalidTopicCategory' is not valid.");
            errors.Single().Fields.Single().Should().Be("gemini.topicCategory");
        }

        [Test]
        public void topic_category_may_be_blank([Values("", null)] string topicCategory)
        {
            var record = BlankRecord().With(r => r.Gemini.TopicCategory = topicCategory);
            var errors = new RecordValidator().Validate(record);
            errors.Should().BeEmpty();
        }

        [Test]
        public void non_open_records_must_describe_their_limitations_on_public_access(
            [Values(Security.Classified, Security.Restricted)] Security security,
            [Values("", " ", null)] string limitationsOnPublicAccess)
        {
            var record = BlankRecord().With(r =>
                {
                    r.Security = security;
                    r.Gemini.LimitationsOnPublicAccess = limitationsOnPublicAccess;
                });

            var errors = new RecordValidator().Validate(record);
            
            errors.Single().Fields.Should().Contain("security");
            errors.Single().Fields.Should().Contain("gemini.limitationsOnPublicAccess");
        }

        // todo

        // 
        // publishable records must have a resourcelocator, which must be a public url
        // responsible party role should be one of code list in ResponsiblePartyRoles.Allowed
        // warning for blank use constraints - could be "no conditions apply" if that's what's meant
        // warning for unknown data format
        // valid email addresses, dates, ...
    }
}
