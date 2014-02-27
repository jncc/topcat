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

            if (record.Path.IsBlank())
                results.Add("Path must not be blank.", r => r.Path);

            if (!IsTopicCategoryValid(record.Gemini.TopicCategory))
                results.Add(
                    String.Format("Topic Category '{0}' is not valid.", record.Gemini.TopicCategory),
                    r => r.Gemini.TopicCategory);

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
                        let fullDottedPath = e.Body.ToString().Replace("r.", String.Empty)
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
        public void should_fail_when_path_is_empty()
        {
            var errors = new RecordValidator().Validate(BlankRecord().With(r => r.Path = ""));

            errors.Single().Message.Should().StartWith("Path must not be blank");
            errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void should_fail_when_path_is_null()
        {
            var errors = new RecordValidator().Validate(BlankRecord().With(r => r.Path = null));

            errors.Single().Message.Should().StartWith("Path must not be blank");
            errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void should_fail_when_path_is_whitespace()
        {
            var errors = new RecordValidator().Validate(BlankRecord().With(r => r.Path = " "));

            errors.Single().Message.Should().StartWith("Path must not be blank");
            errors.Single().Fields.Single().Should().Be("path");
        }

        [Test]
        public void should_fail_when_topic_category_not_valid()
        {
            var record = BlankRecord().With(r => r.Gemini.TopicCategory = "anInvalidTopicCategory");
            var errors = new RecordValidator().Validate(record);

            errors.Single().Message.Should().Contain("Topic Category 'anInvalidTopicCategory' is not valid.");
            errors.Single().Fields.Single().Should().Be("gemini.topicCategory");
        }

        [Test]
        public void should_pass_when_topic_category_is_null()
        {
            var errors = new RecordValidator().Validate(BlankRecord().With(r => r.Gemini.TopicCategory = null));
            errors.Should().BeEmpty();
        }

        [Test]
        public void should_pass_when_topic_category_is_empty()
        {
            var errors = new RecordValidator().Validate(BlankRecord().With(r => r.Gemini.TopicCategory = ""));
            errors.Should().BeEmpty();
        }


        // todo

        // non_open_records_should_have_non_empty_limitations_on_public_access
        // publishable records must have a resourcelocator, which must be a public url
        // responsible party role should be one of code list in ResponsiblePartyRoles.Allowed
        // warning for blank use constraints - could be "no conditions apply" if that's what's meant
        // warning for unknown data format
        // valid email addresses, dates, ...
    }
}
