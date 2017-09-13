using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
                    m.Title = "Uploader test";
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
                        SignOff = null,
                        Paused = false
                    }
                };
                r.Footer = new Footer();
            });

            var nullPublicationRecord = new Record().With(r =>
            {
                r.Id = new Guid("92efc007-e98e-4263-80f3-847c5f9c4e08");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Uploader test";
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

            var assessedAndSignedOffRecord = new Record().With(r =>
            {
                r.Id = new Guid("09ed523e-a35f-4654-a337-64ee732e505f");
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Title = "Uploader test";
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
                            User = new UserInfo
                            {
                                DisplayName = "IAO User",
                                Email = "iaouser@example.com"
                            }
                        },
                        Paused = false
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
                    m.Title = "Uploader test";
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
                            User = new UserInfo
                            {
                                DisplayName = "IAO User",
                                Email = "iaouser@example.com"
                            }
                        },
                        Paused = false
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
                    m.Title = "Uploader test";
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
                            User = new UserInfo
                            {
                                DisplayName = "IAO User",
                                Email = "iaouser@example.com"
                            }
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
                    m.Title = "Uploader test";
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
                            User = new UserInfo
                            {
                                DisplayName = "IAO User",
                                Email = "iaouser@example.com"
                            }
                        },
                        Paused = false,
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 08, 03),
                            Message = null
                        },
                        LastSuccess = null
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
                    m.Title = "Uploader test";
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
                            User = new UserInfo
                            {
                                DisplayName = "IAO User",
                                Email = "iaouser@example.com"
                            }
                        },
                        Paused = false,
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 08, 03),
                            Message = null
                        },
                        LastSuccess = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 08, 03),
                            Message = null
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
                db.SaveChanges();

                Thread.Sleep(100); // Allow time for indexing

                var uploadServiceMock = new Mock<IOpenDataPublishingUploadService>();
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var robotUploader = new RobotUploader(db, uploadServiceMock.Object, uploadHelperMock.Object);

                var pendingRecords = robotUploader.GetRecordsPendingUpload();
                pendingRecords.Count.Should().Be(2);
                pendingRecords.Contains(assessedAndSignedOffRecord).Should().BeTrue();
                pendingRecords.Contains(attemptedButFailedRecord).Should().BeTrue();
            }
        }
    }
}
