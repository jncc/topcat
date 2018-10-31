using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Catalogue.Data;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    class robot_uploader_specs : DatabaseTestFixture
    {
        [Test]
        public void pending_upload_test_with_assessed_record()
        {
            var assessedRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("fc3a649f-0712-4175-9624-77555918ea79");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
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
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(assessedRecord);
        }

        [Test]
        public void pending_upload_test_with_null_publication_record()
        {
            var nullPublicationRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("92efc007-e98e-4263-80f3-847c5f9c4e08");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
                r.Publication = null;
                r.Footer = new Footer();
            });

            TestRecordNotReturned(nullPublicationRecord);
        }

        [Test]
        public void pending_upload_test_with_assessed_and_signed_off_record()
        {
            var assessedAndSignedOffRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("09ed523e-a35f-4654-a337-64ee732e505f");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 26);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Publishable = true,
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 26)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordReturned(assessedAndSignedOffRecord);
        }

        [Test]
        public void pending_upload_test_with_signed_off_only_record()
        {
            var signedOffOnlyRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("cfefc2b9-bd6c-4005-a7a6-c0d66be4a8e0");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
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
                            DateUtc = new DateTime(2017, 09, 25)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(signedOffOnlyRecord);
        }

        [Test]
        public void pending_upload_test_with_paused_record()
        {
            var pausedRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("5775e234-56fb-4ef4-b932-95325fa88674");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
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
                            DateUtc = new DateTime(2017, 09, 26)
                        },
                        Paused = true
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(pausedRecord);
        }

        [Test]
        public void pending_upload_test_with_failed_attempt_record()
        {
            var attemptedButFailedRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("1a4fae84-465d-4823-9db5-494e0eee0de7");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 27);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Publishable = true,
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 26)
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordReturned(attemptedButFailedRecord);
        }

        [Test]
        public void pending_upload_test_with_already_uploaded_record()
        {
            var alreadyUploadedRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("3df7c4ca-2be5-4455-b2e8-dc984d2c3fbe");
                r.Path = @"X:\path\to\uploader\test";
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
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 26)
                        },
                        Paused = false,
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(alreadyUploadedRecord);
        }

        [Test]
        public void pending_upload_test_with_published_and_out_of_date_record()
        {
            var publishedAndOutOfDateRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("98b55f61-964b-4186-8af8-e3d62a2aace4");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 28);
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
                            DateUtc = new DateTime(2017, 09, 26)
                        },
                        Paused = false,
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(publishedAndOutOfDateRecord);
        }

        [Test]
        public void pending_upload_test_with_ready_to_republish_record()
        {
            var readyToRepublishRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("d41f3ffc-0b60-49b4-af15-94e0f3180f29");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Publishable = true,
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 28)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 29)
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordReturned(readyToRepublishRecord);
        }

        [Test]
        public void pending_upload_test_with_published_and_reassessed_record()
        {
            var publishedAndReassessedRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("b41a3f52-05de-4e52-abb6-18a6d835e39f");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 28);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 28)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 26)
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(publishedAndReassessedRecord);
        }

        [Test]
        public void pending_upload_test_with_signed_off_and_out_of_date_record()
        {
            var publishedAssessedSignedOffThenEditedRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("6e48d4c7-3174-409f-a4d4-00d6909b7c8f");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 30);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 28)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 29)
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(publishedAssessedSignedOffThenEditedRecord);
        }

        [Test]
        public void pending_upload_test_with_unpublishable_record()
        {
            var unpublishableRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("7859f8b7-5d27-47f6-af4a-85f2c296beeb");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 26);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Publishable = false,
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 26)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(unpublishableRecord);
        }

        [Test]
        public void pending_upload_test_with_null_publishable_record()
        {
            var unpublishableRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("1e776af7-2c77-4017-b7e9-4d31ad560fd2");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 26);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Publishable = null,
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 25)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 26)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(unpublishableRecord);
        }

        [Test]
        public void pending_reupload_test_with_unpublishable_record()
        {
            var unpublishableRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("f725ebb4-0ef5-4ef6-bded-68d77aee4ad4");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Publishable = false,
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true,
                            CompletedOnUtc = new DateTime(2017, 09, 28)
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 09, 29)
                        },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(unpublishableRecord);
        }

        private void TestRecordNotReturned(Record record)
        {
            var pendingRecords = PendingUploadTest(record);
            pendingRecords.Count.Should().Be(0);
        }

        private void TestRecordReturned(Record record)
        {
            var pendingRecords = PendingUploadTest(record);
            pendingRecords.Count.Should().Be(1);
            pendingRecords.Contains(record).Should().BeTrue();
        }

        private List<Record> PendingUploadTest(Record record)
        {
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();
                WaitForIndexing(ReusableDocumentStore);

                var uploadServiceMock = new Mock<IOpenDataPublishingUploadRecordService>();
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var robotUploader = new RobotUploader(db, uploadServiceMock.Object, uploadHelperMock.Object);

                var result = robotUploader.GetRecordsPendingUpload();
                return result;
            }
        }
    }
}
