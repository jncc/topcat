using Catalogue.Data.Model;
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
    public class robot_uploader_specs : CleanDbTest
    {
        [Test]
        public void pending_upload_test_with_assessed_record()
        {
            var assessedRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Publishable = true,
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Publishable = true,
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Publishable = true,
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Publishable = true,
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Publishable = true,
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
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
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
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

                var uploadServiceMock = new Mock<IPublishingUploadRecordService>();
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var robotUploader = new RobotUploader(db, uploadServiceMock.Object, uploadHelperMock.Object);

                var result = robotUploader.GetRecordsPendingUpload();
                return result;
            }
        }
    }
}
