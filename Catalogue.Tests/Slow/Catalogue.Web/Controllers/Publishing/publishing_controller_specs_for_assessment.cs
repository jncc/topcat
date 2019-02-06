using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Web.Account;
using Catalogue.Web.Controllers.Publishing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raven.Client.Documents.Session;
using System;

namespace Catalogue.Tests.Slow.Catalogue.Web.Controllers.Publishing
{
    public class publishing_controller_specs_for_assessment : CleanDbTest
    {
        [Test]
        public void assessment_completed_for_dgu_publication()
        {
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestAssessment(record);

            resultRecord.Publication.Should().NotBeNull();
            resultRecord.Footer.ModifiedByUser.DisplayName.Should().Be("Test User");
            resultRecord.Footer.ModifiedOnUtc.Should().NotBe(DateTime.MinValue);

            var publicationInfo = resultRecord.Publication;
            publicationInfo.Should().NotBeNull();
            publicationInfo.Gov.LastAttempt.Should().BeNull();
            publicationInfo.Gov.LastSuccess.Should().BeNull();
            resultRecord.Publication.Data.Should().BeNull();
            publicationInfo.Gov.Paused.Should().BeFalse();
            publicationInfo.Assessment.Completed.Should().BeTrue();
            publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Test User");
            publicationInfo.Assessment.CompletedByUser.Email.Should().Be("tester@example.com");
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
            resultRecord.Gemini.MetadataDate.Should().Be(publicationInfo.Assessment.CompletedOnUtc);
        }

        [Test]
        public void assessment_completed_for_hub_publication()
        {
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Hub = new HubPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestAssessment(record);

            resultRecord.Publication.Should().NotBeNull();
            resultRecord.Footer.ModifiedByUser.DisplayName.Should().Be("Test User");
            resultRecord.Footer.ModifiedOnUtc.Should().NotBe(DateTime.MinValue);

            var publicationInfo = resultRecord.Publication;
            publicationInfo.Should().NotBeNull();
            publicationInfo.Data.Should().BeNull();
            publicationInfo.Hub.LastSuccess.Should().BeNull();
            publicationInfo.Hub.LastAttempt.Should().BeNull();
            publicationInfo.Gov.Should().BeNull();
            publicationInfo.Assessment.Completed.Should().BeTrue();
            publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Test User");
            publicationInfo.Assessment.CompletedByUser.Email.Should().Be("tester@example.com");
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
            resultRecord.Gemini.MetadataDate.Should().Be(publicationInfo.Assessment.CompletedOnUtc);
        }

        [Test]
        public void assessment_completed_with_unc_path_test()
        {
            var recordId = Helpers.AddCollection("b69f47c1-4c17-42d0-a396-8209aa5568b1");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"\\jncc-corpfile\testfile";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
                r.Publication = new PublicationInfo
                {
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestAssessment(record);
            var publicationInfo = resultRecord.Publication;
            publicationInfo.Assessment.Completed.Should().BeTrue();
        }

        [Test]
        public void assessment_started_then_completed_test()
        {
            var recordId = Helpers.AddCollection("ec0db5b3-8b9d-42c3-ac70-2fd50ff3bbca");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 21);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = false,
                        CompletedByUser = null,
                        CompletedOnUtc = DateTime.MinValue,
                        InitialAssessmentWasDoneOnSpreadsheet = false
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestAssessment(record);

            resultRecord.Publication.Should().NotBeNull();

            var publicationInfo = resultRecord.Publication;
            publicationInfo.Should().NotBeNull();
            publicationInfo.Gov.LastAttempt.Should().BeNull();
            publicationInfo.Gov.LastSuccess.Should().BeNull();
            resultRecord.Publication.Data.Should().BeNull();
            publicationInfo.Gov.Paused.Should().BeFalse();
            publicationInfo.Assessment.Completed.Should().BeTrue();
            publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Test User");
            publicationInfo.Assessment.CompletedByUser.Email.Should().Be("tester@example.com");
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
        }

        [Test]
        public void assessment_already_completed_test()
        {
            var recordId = Helpers.AddCollection("1a86bbbe-7f19-4fe2-82ff-7847e68266da");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 07, 30)
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            });

            Action a = () => TestAssessment(record);
            a.Should().Throw<InvalidOperationException>().And.Message.Should().Be("Assessment has already been completed and is up to date");
        }

        [Test]
        public void assessment_already_completed_on_spreadsheet_test()
        {
            var recordId = Helpers.AddCollection("170001cf-1117-459d-a554-b1fc031d439c");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        InitialAssessmentWasDoneOnSpreadsheet = true,
                        CompletedOnUtc = DateTime.MinValue
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = DateTime.MinValue
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true,
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 30)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            Action a = () => TestAssessment(record);
            a.Should().Throw<InvalidOperationException>().And.Message.Should().Be("Assessment has already been completed and is up to date");
        }

        [Test]
        public void assessment_fails_with_null_publication_info()
        {
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = null;
                r.Footer = new Footer();
            });

            Action a = () => TestAssessment(record);
            a.Should().Throw<InvalidOperationException>().And.Message.Should().Be("Must select at least one publishing destination");
        }

        [Test]
        public void assessment_fails_with_no_publishable_destinations(
            [Values(false, null)] bool? govPublishable,
            [Values(false)] bool hubPublishable)
        {
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Hub = new HubPublicationInfo {Publishable = hubPublishable},
                    Gov = new GovPublicationInfo {Publishable = govPublishable}
                };
                r.Footer = new Footer();
            });

            Action a = () => TestAssessment(record);
            a.Should().Throw<InvalidOperationException>().And.Message.Should().Be("Must select at least one publishing destination");
        }

        [Test]
        public void assessment_for_non_Gemini_record_should_fail_test()
        {
            var recordId = Helpers.AddCollection("aeda73dc-4723-427d-8555-19558087370a");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Basic;
                r.Gemini = Library.Blank().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            });

            Action a = () => TestAssessment(record);
            a.Should().Throw<InvalidOperationException>().And.Message.Should().Be("Validation level must be Gemini");
        }

        [Test]
        public void assessment_for_dgu_unpublishable_but_hub_publishable_record()
        {
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Hub = new HubPublicationInfo
                    {
                        Publishable = true
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = false
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestAssessment(record);

            resultRecord.Publication.Should().NotBeNull();
            resultRecord.Footer.ModifiedByUser.DisplayName.Should().Be("Test User");
            resultRecord.Footer.ModifiedOnUtc.Should().NotBe(DateTime.MinValue);

            var publicationInfo = resultRecord.Publication;
            publicationInfo.Should().NotBeNull();
            publicationInfo.Assessment.Completed.Should().BeTrue();
            publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Test User");
            publicationInfo.Assessment.CompletedByUser.Email.Should().Be("tester@example.com");
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
            resultRecord.Gemini.MetadataDate.Should().Be(publicationInfo.Assessment.CompletedOnUtc);
        }

        [Test]
        public void assessment_for_dgu_publishable_but_hub_unpublishable_record()
        {
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Hub = new HubPublicationInfo
                    {
                        Publishable = false
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestAssessment(record);

            resultRecord.Publication.Should().NotBeNull();
            resultRecord.Footer.ModifiedByUser.DisplayName.Should().Be("Test User");
            resultRecord.Footer.ModifiedOnUtc.Should().NotBe(DateTime.MinValue);

            var publicationInfo = resultRecord.Publication;
            publicationInfo.Should().NotBeNull();
            publicationInfo.Assessment.Completed.Should().BeTrue();
            publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Test User");
            publicationInfo.Assessment.CompletedByUser.Email.Should().Be("tester@example.com");
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
            resultRecord.Gemini.MetadataDate.Should().Be(publicationInfo.Assessment.CompletedOnUtc);
        }

        [Test]
        public void assessment_for_dgu_and_hub_both_publishable_record()
        {
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Hub = new HubPublicationInfo
                    {
                        Publishable = true
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestAssessment(record);

            resultRecord.Publication.Should().NotBeNull();
            resultRecord.Footer.ModifiedByUser.DisplayName.Should().Be("Test User");
            resultRecord.Footer.ModifiedOnUtc.Should().NotBe(DateTime.MinValue);

            var publicationInfo = resultRecord.Publication;
            publicationInfo.Should().NotBeNull();
            publicationInfo.Assessment.Completed.Should().BeTrue();
            publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Test User");
            publicationInfo.Assessment.CompletedByUser.Email.Should().Be("tester@example.com");
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
            resultRecord.Gemini.MetadataDate.Should().Be(publicationInfo.Assessment.CompletedOnUtc);
        }

        [Test]
        public void successful_reassessment_for_republishing()
        {
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 07, 21),
                        CompletedByUser = new UserInfo
                        {
                            DisplayName = "Pete",
                            Email = "pete@example.com"
                        },
                        InitialAssessmentWasDoneOnSpreadsheet = false
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 07, 22)
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true,
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 23)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestAssessment(record);
            resultRecord.Publication.Should().NotBeNull();

            var publicationInfo = resultRecord.Publication;
            publicationInfo.Should().NotBeNull();
            publicationInfo.Gov.LastAttempt.DateUtc.Should().Be(new DateTime(2017, 07, 23));
            publicationInfo.Gov.LastSuccess.Should().BeNull();
            resultRecord.Publication.Data.Should().BeNull();
            publicationInfo.Gov.Paused.Should().BeFalse();
            publicationInfo.Assessment.Completed.Should().BeTrue();
            publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Test User");
            publicationInfo.Assessment.CompletedByUser.Email.Should().Be("tester@example.com");
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(new DateTime(2017, 07, 21));
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
            publicationInfo.Assessment.InitialAssessmentWasDoneOnSpreadsheet.Should().BeFalse();
        }

        [Test]
        public void fail_when_reassessing_up_to_date_record_at_sign_off_stage()
        {
            var recordId = Helpers.AddCollection(Guid.NewGuid().ToString());
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 07, 21),
                        CompletedByUser = new UserInfo
                        {
                            DisplayName = "Pete",
                            Email = "pete@example.com"
                        },
                        InitialAssessmentWasDoneOnSpreadsheet = false
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 07, 30)
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true,
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 20)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var publishingController = GetTestOpenDataPublishingController(db);

                var request = new AssessmentRequest
                {
                    Id = Helpers.RemoveCollection(record.Id)
                };

                Action a = () => publishingController.Assess(request);
                a.Should().Throw<InvalidOperationException>().And.Message.Should().Be("Assessment has already been completed and is up to date");

                var resultRecord = db.Load<Record>(record.Id);
                resultRecord.Publication.Should().NotBeNull();

                var publicationInfo = resultRecord.Publication;
                publicationInfo.Should().NotBeNull();
                publicationInfo.Assessment.Completed.Should().BeTrue();
                publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Pete");
                publicationInfo.Assessment.CompletedByUser.Email.Should().Be("pete@example.com");
                publicationInfo.Assessment.CompletedOnUtc.Should().Be(new DateTime(2017, 07, 21));
                resultRecord.Gemini.MetadataDate.Should().NotBe(publicationInfo.Assessment.CompletedOnUtc);
            }
        }

        [Test]
        public void fail_when_reassessing_up_to_date_record_at_upload_stage()
        {
            var recordId = Helpers.AddCollection("593177d9-885f-4450-903b-ecb9ea667575");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 07, 21),
                        CompletedByUser = new UserInfo
                        {
                            DisplayName = "Pete",
                            Email = "pete@example.com"
                        },
                        InitialAssessmentWasDoneOnSpreadsheet = false
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 07, 29)
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true,
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 30)
                        }
                    }
                };
                r.Footer = new Footer();
            });
            
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var publishingController = GetTestOpenDataPublishingController(db);

                var request = new AssessmentRequest
                {
                    Id = Helpers.RemoveCollection(record.Id)
                };

                Action a = () => publishingController.Assess(request);
                a.Should().Throw<InvalidOperationException>().And.Message.Should().Be("Assessment has already been completed and is up to date");

                var resultRecord = db.Load<Record>(record.Id);
                resultRecord.Publication.Should().NotBeNull();

                var publicationInfo = resultRecord.Publication;
                publicationInfo.Should().NotBeNull();
                publicationInfo.Assessment.Completed.Should().BeTrue();
                publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Pete");
                publicationInfo.Assessment.CompletedByUser.Email.Should().Be("pete@example.com");
                publicationInfo.Assessment.CompletedOnUtc.Should().Be(new DateTime(2017, 07, 21));
                resultRecord.Gemini.MetadataDate.Should().NotBe(publicationInfo.Assessment.CompletedOnUtc);
            }
        }

        [Test]
        public void successful_reassessment_from_spreadsheet_assessment()
        {
            var recordId = Helpers.AddCollection("39af3c76-8769-4994-bb1a-4f1a1017c0b0");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\assessment\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 07, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 07, 21),
                        CompletedByUser = new UserInfo
                        {
                            DisplayName = "Pete",
                            Email = "pete@example.com"
                        },
                        InitialAssessmentWasDoneOnSpreadsheet = true
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 07, 22)
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true,
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 07, 23)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var resultRecord = TestAssessment(record);
            resultRecord.Publication.Should().NotBeNull();

            var publicationInfo = resultRecord.Publication;
            publicationInfo.Should().NotBeNull();
            publicationInfo.Assessment.Completed.Should().BeTrue();
            publicationInfo.Assessment.CompletedByUser.DisplayName.Should().Be("Test User");
            publicationInfo.Assessment.CompletedByUser.Email.Should().Be("tester@example.com");
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(new DateTime(2017, 07, 21));
            publicationInfo.Assessment.CompletedOnUtc.Should().NotBe(DateTime.MinValue);
            publicationInfo.Assessment.InitialAssessmentWasDoneOnSpreadsheet.Should().BeTrue();
            resultRecord.Gemini.MetadataDate.Should().Be(publicationInfo.Assessment.CompletedOnUtc);
        }

        static readonly Record[] NotEligibleForAssessmentRecords =
        {
            new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("dc370d41-c8b4-4eba-8e39-6e2d70c50c07");
                r.Path = @"http://www.example.com";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
                r.Publication = new PublicationInfo
                {
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            }),
            new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("60df47fc-d4df-48ce-9bdd-289c145f7de0");
                r.Path = @"https://www.example.com";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
                r.Publication = new PublicationInfo
                {
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            }),
            new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("d82afb6c-2699-4570-a72f-cdf2cf93fa4c");
                r.Path = @"postgres://username@hostname/databasename";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
                r.Publication = new PublicationInfo
                {
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                };
                r.Footer = new Footer();
            })
        };

        private Record TestAssessment(Record record)
        {
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var publishingController = GetTestOpenDataPublishingController(db);

                var request = new AssessmentRequest
                {
                    Id = Helpers.RemoveCollection(record.Id)
                };

                return ((RecordServiceResult) publishingController.Assess(request)).Record;
            }
        }

        private static PublishingController GetTestOpenDataPublishingController(IDocumentSession db)
        {
            var testUserContext = new TestUserContext();
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(u => u.User).Returns(testUserContext.User);

            var publishingService = new RecordPublishingService(db, new RecordValidator());

            return new PublishingController(db, publishingService, userContextMock.Object);
        }
    }
}
