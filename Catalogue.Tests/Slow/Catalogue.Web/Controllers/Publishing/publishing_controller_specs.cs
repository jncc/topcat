using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Web.Account;
using Catalogue.Web.Controllers.Publishing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Raven.Client;

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
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Open data assessment test - normal scenario";
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

            var resultRecord = TestAssessment(record);

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
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Open data assessment test - assessment completed after being started";
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

            var resultRecord = TestAssessment(record);

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
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Open data assessment test - record with assessment already completed";
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

            TestAssessment(record);
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
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Open data assessment test - record with initial assessment completed on spreadsheet";
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

            TestAssessment(record);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Validation level must be Gemini.")]
        public void assessment_for_non_Gemini_record_should_fail_test()
        {
            var recordId = new Guid("aeda73dc-4723-427d-8555-19558087370a");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Basic;
                r.Gemini = Library.Blank().With(m =>
                {
                    m.Title = "Open data assessment test - record with basic validation";
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

            TestAssessment(record);
        }

        [Test]
        public void successful_open_data_sign_off_test()
        {
            var recordId = new Guid("f34de2d3-17af-47e2-8deb-a16b67c76b06");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Open data sign off test - normal scenario";
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

            var resultRecord = TestSignOff(record);
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
        public void sign_off_with_incomplete_risk_assessment_test()
        {
            var recordId = new Guid("9f9d7a83-8fcb-4afc-956b-3d874d5632b1");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Open data sign off test - record with incomplete risk assessment";
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

            TestSignOff(record);
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
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Open data sign off test - record already signed off";
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

            TestSignOff(record);
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
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Open data sign off test - record fails validation";
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

            TestSignOff(record);
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
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Open data sign off test - record without risk assessment";
                });
                r.Footer = new Footer();
            });

            TestSignOff(record);
        }

        [Test]
        public void awaiting_sign_off_test()
        {
            var retrieveSignOffTest1Record = new Record().With(r =>
            {
                r.Id = new Guid("af8e531f-2bed-412e-9b03-2b339c672bff");
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Retrieve Sign Off Test 1";
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
                            Completed = true
                        },
                        SignOff = null
                    }
                };
                r.Footer = new Footer();
            });

            var retrieveSignOffTest2Record = new Record().With(r =>
            {
                r.Id = new Guid("f4b6dd32-93ad-41cd-a7a0-2df0f5c7410b");
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Retrieve Sign Off Test 2";
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
                r.Publication = null;
                r.Footer = new Footer();
            });

            var retrieveSignOffTest3Record = new Record().With(r =>
            {
                r.Id = new Guid("dbb9bf6e-c128-4611-bd3f-73bd7a9ae4e9");
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Retrieve Sign Off Test 3";
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
                            Completed = true
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 08, 02),
                            User = "Ulric"
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var retrieveSignOffTest4Record = new Record().With(r =>
            {
                r.Id = new Guid("e1255428-90ec-4d8e-a9d9-0cf210c64dbd");
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Retrieve Sign Off Test 4";
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
                            Completed = false
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 08, 02),
                            User = "Ulric"
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var testRecords = new List<Record>(new [] {retrieveSignOffTest1Record, retrieveSignOffTest2Record, retrieveSignOffTest3Record, retrieveSignOffTest4Record});

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                foreach (var record in testRecords)
                {
                    db.Store(record);
                    db.SaveChanges();
                }

                var publishingController = GetTestOpenDataPublishingController(db);
                var result = publishingController.PendingSignOff();
                result.Count.Should().Be(1);
                result[0].Title.Should().Be("Retrieve Sign Off Test 1");
            }
        }

        private static Record TestAssessment(Record record)
        {
            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var publishingController = GetTestOpenDataPublishingController(db);

                var request = new AssessmentRequest
                {
                    Id = record.Id
                };

                return publishingController.Assess(request).Record;
            }
        }

        private static Record TestSignOff(Record record)
        {
            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var publishingController = GetTestOpenDataPublishingController(db);

                var request = new SignOffRequest
                {
                    Id = record.Id,
                    Comment = "Sign off test"
                };

                publishingController.SignOff(request);

                return db.Load<Record>(record.Id);
            }
        }

        private static OpenDataPublishingController GetTestOpenDataPublishingController(IDocumentSession db)
        {
            var testUserContext = new TestUserContext();
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.User).Returns(testUserContext.User);

            var recordService = new RecordService(db, new RecordValidator());
            var publishingService = new OpenDataPublishingService(db, recordService);

            return new OpenDataPublishingController(db, publishingService, userContextMock.Object);
        }
    }
}
