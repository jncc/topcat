using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Helpers;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Collections;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;

namespace Catalogue.Tests.Slow.Catalogue.Data.Write
{
    class gemini_record_validator_specs
    {
        private Record GeminiRecord()
        {
            return new Record
            {
                Path = @"X:\some\path",
                Gemini = Library.Example().With(m =>
                {
                    m.Keywords = new StringPairList
                        {
                            { "http://vocab.jncc.gov.uk/jncc-domain", "Example Domain" },
                            { "http://vocab.jncc.gov.uk/jncc-category", "Example Category" },
                        }
                        .ToKeywordList();
                }),

                Validation = Validation.Gemini,
            };
        }

        [Test]
        public void should_validate_valid_gemini_record()
        {
            var record = GeminiRecord();

            var result = new RecordValidator().Validate(record);

            result.Errors.Count.Should().Be(0);
        }

        [Test]
        public void should_validate_valid_nonspatial_record_with_no_bbox()
        {
            // to keep things comceptually simple, let's try to keep one level of "good" validation - "gemini"
            // but use it for non-spatial metadata too

            var record = GeminiRecord().With(r =>
            {
                r.Gemini.ResourceType = "nonGeographicDataset";
                r.Gemini.BoundingBox = null;
            });

            var result = new RecordValidator().Validate(record);

            result.Errors.Count.Should().Be(0);
        }

        [Test]
        public void responsible_organisation_role_must_not_be_blank()
        {
            var record = GeminiRecord().With(r => r.Gemini.ResponsibleOrganisation = new ResponsibleParty
            {
                Email = "a.mann@example.com",
                Name = "A. Mann",
                Role = "",
            });
            var result = new RecordValidator().Validate(record);
            result.Errors.Single().Fields.Should().Contain("gemini.responsibleOrganisation.role");
        }

        [Test]
        public void blank_lineage_is_not_allowed([Values("", " ", null)] string blank)
        {
            var record = GeminiRecord().With(r => r.Gemini.Lineage = blank);
            var result = new RecordValidator().Validate(record);

            result.Errors.Any(e => e.Fields.Contains("gemini.lineage")).Should().BeTrue();
        }

        [Test]
        public void blank_use_constraints_are_not_allowed([Values("", " ", null)] string blank)
        {
            var record = GeminiRecord().With(r => r.Gemini.UseConstraints = blank);
            var result = new RecordValidator().Validate(record);

            result.Errors.Any(e => e.Fields.Contains("gemini.useConstraints")).Should().BeTrue();
        }

        [Test]
        public void topic_category_must_not_be_blank([Values("", " ", null)] string blank)
        {
            var record = GeminiRecord().With(r => r.Gemini.TopicCategory = blank);
            var result = new RecordValidator().Validate(record);

            result.Errors.Any(e => e.Fields.Contains("gemini.topicCategory")).Should().BeTrue();
        }

        //        [Test]
        //        public void should_not_allow_keyword_additions_to_controlled_vocabs()
    }
}
