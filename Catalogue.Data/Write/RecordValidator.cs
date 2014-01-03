using System;
using System.Collections.Generic;
using System.Linq;
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
        RecordValidationResult Validate(Record record);
    }

    public class RecordValidator : IRecordValidator
    {
        public RecordValidationResult Validate(Record record)
        {
            if (record.Path.IsBlank())
                return MakeFailureResult("Path must not be blank.");

            if (!IsTopicCategoryValid(record.Gemini.TopicCategory))
                return MakeFailureResult("Topic Category '{0}' is not valid.", record.Gemini.TopicCategory);

            return new RecordValidationResult { Success = true };
        }

        static RecordValidationResult MakeFailureResult(string message, params object[] args)
        {
            return new RecordValidationResult
                {
                    Message = String.Format(message, args),
                    Success = false,
                };
        }

        static bool IsTopicCategoryValid(string s)
        {
            return s.IsBlank() || TopicCategories.Values.Keys.Any(k => k == s);
        }
    }


    public class RecordValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    class validator_specs
    {
        Record validBlankRecord = new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Blank()
            };

        [Test]
        public void should_fail_validation_for_invalid_topic_category()
        {
            var record = validBlankRecord.With(r => r.Gemini.TopicCategory = "anInvalidTopicCategory");
            var result = new RecordValidator().Validate(record);

            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Topic Category 'anInvalidTopicCategory' is not valid.");
        }

        [Test]
        public void should_pass_validation_for_null_topic_category()
        {
            var record = validBlankRecord.With(r => r.Gemini.TopicCategory = null);
            var result = new RecordValidator().Validate(record);
            result.Success.Should().BeTrue();
        }

        [Test]
        public void should_pass_validation_for_empty_topic_category()
        {
            var result = new RecordValidator().Validate(validBlankRecord.With(r => r.Gemini.TopicCategory = ""));
            result.Success.Should().BeTrue();
        }
    }
}
