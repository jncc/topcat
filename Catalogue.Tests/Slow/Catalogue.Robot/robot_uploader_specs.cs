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
using System.Threading;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    class robot_uploader_specs
    {
        [Test]
        public void get_records_pending_upload_test_with_assessed_record()
        {
            var assessedRecord = new Record().With(r =>
            {
                r.Id = new Guid("fc3a649f-0712-4175-9624-77555918ea79");
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

            var nullPublicationRecord = new Record().With(r =>
            {
                r.Id = new Guid("92efc007-e98e-4263-80f3-847c5f9c4e08");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
                r.Publication = null;
                r.Footer = new Footer();
            });

            var assessedAndSignedOffRecord = new Record().With(r =>
            {
                r.Id = new Guid("09ed523e-a35f-4654-a337-64ee732e505f");
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
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var signedOffOnlyRecord = new Record().With(r =>
            {
                r.Id = new Guid("cfefc2b9-bd6c-4005-a7a6-c0d66be4a8e0");
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

            var pausedRecord = new Record().With(r =>
            {
                r.Id = new Guid("5775e234-56fb-4ef4-b932-95325fa88674");
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

            var attemptedButFailedRecord = new Record().With(r =>
            {
                r.Id = new Guid("1a4fae84-465d-4823-9db5-494e0eee0de7");
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
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 27)
                        }
                    }
                };
                r.Footer = new Footer();
            });

            var alreadyUploadedRecord = new Record().With(r =>
            {
                r.Id = new Guid("3df7c4ca-2be5-4455-b2e8-dc984d2c3fbe");
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

            var publishedAndOutOfDateRecord = new Record().With(r =>
            {
                r.Id = new Guid("98b55f61-964b-4186-8af8-e3d62a2aace4");
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

            var readyToRepublishRecord = new Record().With(r =>
            {
                r.Id = new Guid("d41f3ffc-0b60-49b4-af15-94e0f3180f29");
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

            var publishedAndReassessedRecord = new Record().With(r =>
            {
                r.Id = new Guid("b41a3f52-05de-4e52-abb6-18a6d835e39f");
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

            var publishedAssessedSignedOffThenEditedRecord = new Record().With(r =>
            {
                r.Id = new Guid("6e48d4c7-3174-409f-a4d4-00d6909b7c8f");
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

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(assessedRecord);
                db.Store(nullPublicationRecord);
                db.Store(assessedAndSignedOffRecord);
                db.Store(signedOffOnlyRecord);
                db.Store(pausedRecord);
                db.Store(attemptedButFailedRecord);
                db.Store(alreadyUploadedRecord);
                db.Store(publishedAndOutOfDateRecord);
                db.Store(readyToRepublishRecord);
                db.Store(publishedAndReassessedRecord);
                db.Store(publishedAssessedSignedOffThenEditedRecord);
                db.SaveChanges();

                Thread.Sleep(100); // Allow time for indexing

                var uploadServiceMock = new Mock<IOpenDataPublishingUploadService>();
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var robotUploader = new RobotUploader(db, uploadServiceMock.Object, uploadHelperMock.Object);

                var pendingRecords = robotUploader.GetRecordsPendingUpload();
                pendingRecords.Count.Should().Be(3);
                pendingRecords.Contains(assessedAndSignedOffRecord).Should().BeTrue();
                pendingRecords.Contains(attemptedButFailedRecord).Should().BeTrue();
                pendingRecords.Contains(readyToRepublishRecord).Should().BeTrue();
            }
        }
    }
}
