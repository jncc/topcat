using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Data.Write
{
    public interface IVocabularyValidator
    {
        ValidationResult<Vocabulary> Validate(Vocabulary vocab);
    }

    public class VocabularyValidator : IVocabularyValidator
    {
        public ValidationResult<Vocabulary> Validate(Vocabulary vocab)
        {

            var result = new ValidationResult<Vocabulary>();

            //validate name
            if (String.IsNullOrWhiteSpace(vocab.Name))
            {
                result.Errors.Add("The name must not be blank", v => v.Name);
            }

            return result;
        }
    }

    internal class when_validating_vocabulary
    {
        private Vocabulary SimpleVocab()
        {
            return new Vocabulary
            {
                Id = "http://some/vocab",
                Name = "Some vocab",
                Keywords = new List<VocabularyKeyword>
                    {
                        new VocabularyKeyword {Value = "Some keyword"}
                    }
            };
        }

        [Test]
        public void should_validate_by_default()
        {
            var result = new VocabularyValidator().Validate(SimpleVocab());

            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Test]
        public void name_must_not_be_blank()
        {
            var v = SimpleVocab();
            v.Name = String.Empty;

            var result = new VocabularyValidator().Validate(v);

            result.Errors.Single().Message.Should().StartWith("The name must not be blank");
            result.Errors.Single().Fields.Single().Should().Be("name");

        }
    }

}
