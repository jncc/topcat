using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Web.Account;
using Catalogue.Web.Controllers.Publishing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Publishing
{
    class publishing_controller_specs_for_sign_off
    {
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
                    m.MetadataDate = new DateTime(2017, 09, 27);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 27)
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
            openDataInfo.SignOff.User.DisplayName.Should().Be("Test User");
            openDataInfo.SignOff.User.Email.Should().Be("tester@example.com");
            openDataInfo.SignOff.DateUtc.Should().NotBe(DateTime.MinValue);
            openDataInfo.SignOff.Comment.Should().Be("Sign off test");
        }

        [Test]
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
                    m.MetadataDate = new DateTime(2017, 09, 27);
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

            Action a = () => TestSignOff(record);
            a.ShouldThrow<InvalidOperationException>().And.Message.Should().Be("Couldn't sign-off record for publication - assessment not completed or out of date");
        }

        [Test]
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
                    m.MetadataDate = new DateTime(2017, 07, 20);
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
                            User = new UserInfo
                            {
                                DisplayName = "Ulric",
                                Email = "ulric@jncc.gov.uk"
                            }
                        }
                    }
                };
                r.Footer = new Footer();
            });

            Action a = () => TestSignOff(record);
            a.ShouldThrow<InvalidOperationException>().And.Message.Should().Be("The record has already been signed off");
        }

        [Test]
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
                    m.Title = "A very long title that is over two hundred characters long so that the Gemini " +
                              "validation will fail.........................................................." +
                              "..............................................................................";
                    m.MetadataDate = new DateTime(2017, 09, 27);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 27)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            Action a = () => TestSignOff(record);
            a.ShouldThrow<Exception>().And.Message.Should().Be("Error while saving sign off changes");
        }

        [Test]
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
                    m.MetadataDate = new DateTime(2017, 09, 27);
                });
                r.Footer = new Footer();
            });

            Action a = () => TestSignOff(record);
            a.ShouldThrow<InvalidOperationException>().And.Message.Should().Be("Couldn't sign-off record for publication - assessment not completed or out of date");
        }

        [Test]
        public void successful_sign_off_for_republishing()
        {
            var recordId = new Guid("b288b636-026b-4187-96d4-a083e9cbe9e4");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 12);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 07, 12)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            User = new UserInfo
                            {
                                DisplayName = "Cathy",
                                Email = "cathy@example.com"
                            },
                            DateUtc = new DateTime(2017, 07, 10)
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 11)
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 11)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestSignOff(record);
            resultRecord.Publication.Should().NotBeNull();

            var openDataInfo = resultRecord.Publication.OpenData;
            openDataInfo.Should().NotBeNull();
            openDataInfo.LastAttempt.DateUtc.Should().Be(new DateTime(2017, 07, 11));
            openDataInfo.LastSuccess.DateUtc.Should().Be(new DateTime(2017, 07, 11));
            openDataInfo.Resources.Should().BeNull();
            openDataInfo.Paused.Should().BeFalse();
            openDataInfo.SignOff.User.DisplayName.Should().Be("Test User");
            openDataInfo.SignOff.User.Email.Should().Be("tester@example.com");
            openDataInfo.SignOff.DateUtc.Should().NotBe(DateTime.MinValue);
            openDataInfo.SignOff.DateUtc.Should().NotBe(new DateTime(2017, 07, 10));
        }

        [Test]
        public void sign_off_when_already_signed_off()
        {
            var recordId = new Guid("84967e72-0a01-49f1-8793-b5a36df3d0be");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 12);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 07, 09)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            User = new UserInfo
                            {
                                DisplayName = "Cathy",
                                Email = "cathy@example.com"
                            },
                            DateUtc = new DateTime(2017, 07, 10)
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 12)
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 12)
                        }
                    }
                };
                r.Footer = new Footer();
            });

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
                
                Action a = () => publishingController.SignOff(request);
                a.ShouldThrow<InvalidOperationException>().And.Message.Should().Be("The record has already been signed off");

                var resultRecord = db.Load<Record>(record.Id);
                resultRecord.Publication.Should().NotBeNull();

                var openDataInfo = resultRecord.Publication.OpenData;
                openDataInfo.Should().NotBeNull();
                openDataInfo.LastAttempt.DateUtc.Should().Be(new DateTime(2017, 07, 12));
                openDataInfo.LastSuccess.DateUtc.Should().Be(new DateTime(2017, 07, 12));
                openDataInfo.Resources.Should().BeNull();
                openDataInfo.Paused.Should().BeFalse();
                openDataInfo.SignOff.User.DisplayName.Should().Be("Cathy");
                openDataInfo.SignOff.User.Email.Should().Be("cathy@example.com");
                openDataInfo.SignOff.DateUtc.Should().Be(new DateTime(2017, 07, 10));
            }
        }

        [Test]
        public void sign_off_record_with_failed_upload_attempt()
        {
            var recordId = new Guid("5d8ce359-4475-4a0e-9f31-0f70dbbc8bfc");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 12);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 07, 09)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            User = new UserInfo
                            {
                                DisplayName = "Cathy",
                                Email = "cathy@example.com"
                            },
                            DateUtc = new DateTime(2017, 07, 10)
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 12)
                        }
                    }
                };
                r.Footer = new Footer();
            });

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

                Action a = () => publishingController.SignOff(request);
                a.ShouldThrow<InvalidOperationException>().And.Message.Should().Be("The record has already been signed off");

                var resultRecord = db.Load<Record>(record.Id);
                resultRecord.Publication.Should().NotBeNull();

                var openDataInfo = resultRecord.Publication.OpenData;
                openDataInfo.Should().NotBeNull();
                openDataInfo.LastAttempt.DateUtc.Should().Be(new DateTime(2017, 07, 12));
                openDataInfo.LastSuccess.Should().BeNull();
                openDataInfo.Resources.Should().BeNull();
                openDataInfo.Paused.Should().BeFalse();
                openDataInfo.SignOff.User.DisplayName.Should().Be("Cathy");
                openDataInfo.SignOff.User.Email.Should().Be("cathy@example.com");
                openDataInfo.SignOff.DateUtc.Should().Be(new DateTime(2017, 07, 10));
            }
        }

        [Test]
        public void sign_off_out_of_date_record()
        {
            var recordId = new Guid("d2183557-a36b-4cfb-8a57-279febdc4de5");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\signoff\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 12);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 07, 09)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            User = new UserInfo
                            {
                                DisplayName = "Cathy",
                                Email = "cathy@example.com"
                            },
                            DateUtc = new DateTime(2017, 07, 10)
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 11)
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 11)
                        }
                    }
                };
                r.Footer = new Footer();
            });

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

                Action a = () => publishingController.SignOff(request);
                a.ShouldThrow<InvalidOperationException>().And.Message.Should().Be("Couldn't sign-off record for publication - assessment not completed or out of date");

                var resultRecord = db.Load<Record>(record.Id);
                resultRecord.Publication.Should().NotBeNull();

                var openDataInfo = resultRecord.Publication.OpenData;
                openDataInfo.Should().NotBeNull();
                openDataInfo.LastAttempt.DateUtc.Should().Be(new DateTime(2017, 07, 11));
                openDataInfo.LastSuccess.DateUtc.Should().Be(new DateTime(2017, 07, 11));
                openDataInfo.Resources.Should().BeNull();
                openDataInfo.Paused.Should().BeFalse();
                openDataInfo.SignOff.User.DisplayName.Should().Be("Cathy");
                openDataInfo.SignOff.User.Email.Should().Be("cathy@example.com");
                openDataInfo.SignOff.DateUtc.Should().Be(new DateTime(2017, 07, 10));
            }
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
                    m.MetadataDate = new DateTime(2017, 09, 25);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
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
                    m.MetadataDate = new DateTime(2017, 09, 25);
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
                    m.MetadataDate = new DateTime(2017, 09, 26);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 26),
                            User = TestUserInfo.TestUser
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
                    m.MetadataDate = new DateTime(2017, 09, 25);
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
                            DateUtc = new DateTime(2017, 09, 25),
                            User = TestUserInfo.TestUser
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var retrieveSignOffTest5Record = new Record().With(r =>
            {
                r.Id = new Guid("f37efe7f-3033-42d4-83a0-f6d7ab59d0c2");
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Retrieve Sign Off Test 5";
                    m.MetadataDate = new DateTime(2017, 09, 25);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 20),
                            User = TestUserInfo.TestUser
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 21)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var retrieveSignOffTest6Record = new Record().With(r =>
            {
                r.Id = new Guid("d038b054-269e-4d4f-a635-da75929e8fee");
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Retrieve Sign Off Test 6";
                    m.MetadataDate = new DateTime(2017, 09, 30);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 28),
                            User = TestUserInfo.TestUser
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 26)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var retrieveSignOffTest7Record = new Record().With(r =>
            {
                r.Id = new Guid("7ec978bc-2ecd-4ab4-a233-5aead4947ab2");
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Retrieve Sign Off Test 7";
                    m.MetadataDate = new DateTime(2017, 09, 25);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = false,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 20),
                            User = TestUserInfo.TestUser
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 21)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var retrieveSignOffTest8Record = new Record().With(r =>
            {
                r.Id = new Guid("fd32ba72-41d4-4769-a365-34ad570fbf7b");
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Retrieve Sign Off Test 8";
                    m.MetadataDate = new DateTime(2017, 09, 30);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 26),
                            User = TestUserInfo.TestUser
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 30)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var testRecords = new List<Record>(new [] {retrieveSignOffTest1Record, retrieveSignOffTest2Record,
                retrieveSignOffTest3Record, retrieveSignOffTest4Record, retrieveSignOffTest5Record,
                retrieveSignOffTest6Record, retrieveSignOffTest7Record, retrieveSignOffTest8Record});

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                foreach (var record in testRecords)
                {
                    db.Store(record);
                }
                db.SaveChanges();

                Thread.Sleep(100); // Allow time for indexing

                var publishingController = GetTestOpenDataPublishingController(db);
                var result = publishingController.PendingSignOff();
                result.Count.Should().Be(2);
                result.Count(r => string.Equals(r.Title, "Retrieve Sign Off Test 1", StringComparison.CurrentCulture)).Should().Be(1);
                result.Count(r => string.Equals(r.Title, "Retrieve Sign Off Test 5", StringComparison.CurrentCulture)).Should().Be(1);
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

                return publishingController.SignOff(request).Record;
            }
        }

        private static OpenDataPublishingController GetTestOpenDataPublishingController(IDocumentSession db)
        {
            var testUserContext = new TestUserContext();
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.User).Returns(testUserContext.User);

            var recordService = new RecordService(db, new RecordValidator());
            var publishingService = new OpenDataPublishingService(recordService);

            return new OpenDataPublishingController(db, publishingService, userContextMock.Object);
        }
    }
}
