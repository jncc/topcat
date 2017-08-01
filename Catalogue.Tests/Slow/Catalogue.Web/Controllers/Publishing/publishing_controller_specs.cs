using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using NUnit.Framework;
using System;
using Catalogue.Gemini.Model;
using Catalogue.Web.Account;
using Catalogue.Web.Controllers.Publishing;
using Moq;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Publishing
{
    class publishing_controller_specs
    {
        [Test]
        public void assessment_completed_test()
        {
            var recordId = new Guid("1a86bbbe-7f19-4fe2-82ff-7847e68266da");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data assessment test";
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-domain",
                        Value = "Terrestrial"
                    });
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                        Value = "Example Collection"
                    });
                });
                r.Footer = new Footer();
            });

            var resultRecord = TestPublishingStage(record, recordId, "Assessment");

            resultRecord.Publication.Should().NotBeNull();
            resultRecord.Footer.ModifiedBy.Should().Be("Test User");
            resultRecord.Footer.ModifiedOnUtc.Should().NotBe(DateTime.MinValue);

            var openDataInfo = resultRecord.Publication.OpenData;
            openDataInfo.Should().NotBeNull();
            openDataInfo.LastAttempt.Should().BeNull();
            openDataInfo.LastSuccess.Should().BeNull();
            openDataInfo.Resources.Should().BeNull();
            openDataInfo.Paused.Should().BeFalse();
            openDataInfo.Assessment.Completed.Should().BeTrue();
            openDataInfo.Assessment.CompletedBy.Should().Be("Test User");
            openDataInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
        }

        [Test]
        public void assessment_started_then_completed_test()
        {
            var recordId = new Guid("ec0db5b3-8b9d-42c3-ac70-2fd50ff3bbca");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data assessment test";
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-domain",
                        Value = "Terrestrial"
                    });
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                        Value = "Example Collection"
                    });
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = false,
                            CompletedBy = null,
                            CompletedOnUtc = DateTime.MinValue,
                            InitialAssessmentWasDoneOnSpreadsheet = false
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestPublishingStage(record, recordId, "Assessment");

            resultRecord.Publication.Should().NotBeNull();

            var openDataInfo = resultRecord.Publication.OpenData;
            openDataInfo.Should().NotBeNull();
            openDataInfo.LastAttempt.Should().BeNull();
            openDataInfo.LastSuccess.Should().BeNull();
            openDataInfo.Resources.Should().BeNull();
            openDataInfo.Paused.Should().BeFalse();
            openDataInfo.Assessment.Completed.Should().BeTrue();
            openDataInfo.Assessment.CompletedBy.Should().Be("Test User");
            openDataInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Assessment has already been completed.")]
        public void assessment_already_completed_test()
        {
            var recordId = new Guid("1a86bbbe-7f19-4fe2-82ff-7847e68266da");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data assessment test";
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-domain",
                        Value = "Terrestrial"
                    });
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                        Value = "Example Collection"
                    });
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestPublishingStage(record, recordId, "Assessment");
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Assessment has already been completed.")]
        public void assessment_already_completed_on_spreadsheet_test()
        {
            var recordId = new Guid("170001cf-1117-459d-a554-b1fc031d439c");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data assessment test";
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-domain",
                        Value = "Terrestrial"
                    });
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                        Value = "Example Collection"
                    });
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            InitialAssessmentWasDoneOnSpreadsheet = true
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestPublishingStage(record, recordId, "Assessment");
        }

        [Test]
        public void successful_open_data_sign_off_test()
        {
            var recordId = new Guid("f34de2d3-17af-47e2-8deb-a16b67c76b06");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data sign off test";
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-domain",
                        Value = "Terrestrial"
                    });
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                        Value = "Example Collection"
                    });
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestPublishingStage(record, recordId, "Sign off");
            resultRecord.Publication.Should().NotBeNull();

            var openDataInfo = resultRecord.Publication.OpenData;
            openDataInfo.Should().NotBeNull();
            openDataInfo.LastAttempt.Should().BeNull();
            openDataInfo.LastSuccess.Should().BeNull();
            openDataInfo.Resources.Should().BeNull();
            openDataInfo.Paused.Should().BeFalse();
            openDataInfo.SignOff.User.Should().Be("Test User");
            openDataInfo.SignOff.DateUtc.Should().NotBe(DateTime.MinValue);
            openDataInfo.SignOff.Comment.Should().Be("Sign off test");
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't sign-off record for publication. Assessment not completed.")]
        public void sign_off_without_risk_assessment_test()
        {
            var recordId = new Guid("9f9d7a83-8fcb-4afc-956b-3d874d5632b1");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data sign off test";
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-domain",
                        Value = "Terrestrial"
                    });
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                        Value = "Example Collection"
                    });
                });
                r.Footer = new Footer();
            });

            TestPublishingStage(record, recordId, "Sign off");
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't sign-off record for publication. Assessment not completed.")]
        public void sign_off_with_incomplete_risk_assessment_test()
        {
            var recordId = new Guid("9f9d7a83-8fcb-4afc-956b-3d874d5632b1");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data sign off test";
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-domain",
                        Value = "Terrestrial"
                    });
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                        Value = "Example Collection"
                    });
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = false,
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestPublishingStage(record, recordId, "Sign off");
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "The record has already been signed off and cannot be signed off again.")]
        public void repeat_sign_off_should_fail_test()
        {
            var recordId = new Guid("eb6fc4d3-1d75-446d-adc8-296881110079");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data sign off test";
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-domain",
                        Value = "Terrestrial"
                    });
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                        Value = "Example Collection"
                    });
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 07, 20),
                            User = "Ulric"
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestPublishingStage(record, recordId, "Sign off");
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Error while saving sign off changes.")]
        public void failure_when_saving_sign_off_changes_test()
        {
            var recordId = new Guid("30f9aed6-62f2-478d-8851-c322ddb7beb8");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data sign off test";
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestPublishingStage(record, recordId, "Sign off");
        }

        private static Record TestPublishingStage(Record record, Guid recordId, string publishingStage)
        {
            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var testUserContext = new TestUserContext();
                var userContextMock = new Mock<IUserContext>();
                userContextMock.Setup(u => u.User).Returns(testUserContext.User);

                var recordService = new RecordService(db, new RecordValidator());
                var publishingService = new OpenDataPublishingService(db, recordService);
                var publishingController =
                    new OpenDataPublishingController(db, publishingService, userContextMock.Object);

                switch (publishingStage)
                {
                    case "Assessment":
                        return TestAssessment(publishingController, recordId);
                    case "Sign off":
                        TestSignOff(publishingController, recordId);
                        break;
                }

                return db.Load<Record>(recordId);
            }
        }

        private static Record TestAssessment(OpenDataPublishingController publishingController, Guid recordId)
        {
            var request = new AssessmentRequest()
            {
                Id = recordId
            };
            var result = publishingController.Assess(request);
            return result.Record;
        }

        private static void TestSignOff(OpenDataPublishingController publishingController, Guid recordId)
        {
            var request = new SignOffRequest
            {
                Id = recordId,
                Comment = "Sign off test"
            };
            publishingController.SignOff(request);
        }
    }
}
