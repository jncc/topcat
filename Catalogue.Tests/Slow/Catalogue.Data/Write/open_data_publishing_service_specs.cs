using Catalogue.Data.Model;
using Catalogue.Data.Test;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Time;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using Catalogue.Robot.Publishing.OpenData;
using Moq;

namespace Catalogue.Tests.Slow.Catalogue.Data.Write
{
    class open_data_publishing_service_specs
    {
        [Test]
        public void successful_upload()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("eb189a2f-ebce-4232-8dc6-1ad486cacf21");
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.ResourceLocator = null;
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
                            User = TestUserInfo.TestUser
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

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new OpenDataPublishingUploadService(new RecordService(db, new RecordValidator()));
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);

                uploadHelperMock.Setup(x => x.GetHttpRootUrl()).Returns("http://data.jncc.gov.uk");

                uploader.Upload(new List<Record>{record});

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.OpenData.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastAttempt.Message.Should().BeNull();
                updatedRecord.Publication.OpenData.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastSuccess.Message.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                updatedRecord.Gemini.ResourceLocator.Should().Be("http://data.jncc.gov.uk/data/eb189a2f-ebce-4232-8dc6-1ad486cacf21-test");
                uploadHelperMock.Verify(x => x.UploadDataFile(record.Id, record.Path), Times.Once);
                uploadHelperMock.Verify(x => x.UploadMetadataDocument(record), Times.Once);
                uploadHelperMock.Verify(x => x.UploadWafIndexDocument(record), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void successful_upload_with_additional_resources()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("eb189a2f-ebce-4232-8dc6-1ad486cacf21");
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.ResourceLocator = null;
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
                            User = TestUserInfo.TestUser
                        },
                        Resources = new List<Resource>{ new Resource{ Path = "x:\\test\\path" } }
                    }
                };
                r.Footer = new Footer();
            });

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new OpenDataPublishingUploadService(new RecordService(db, new RecordValidator()));
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);

                uploadHelperMock.Setup(x => x.GetHttpRootUrl()).Returns("http://data.jncc.gov.uk");

                uploader.Upload(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.OpenData.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastAttempt.Message.Should().BeNull();
                updatedRecord.Publication.OpenData.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastSuccess.Message.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                updatedRecord.Gemini.ResourceLocator.Should().BeNull();
                uploadHelperMock.Verify(x => x.UploadAlternativeResources(record), Times.Once);
                uploadHelperMock.Verify(x => x.UploadDataFile(record.Id, record.Path), Times.Never);
                uploadHelperMock.Verify(x => x.UploadMetadataDocument(record), Times.Once);
                uploadHelperMock.Verify(x => x.UploadWafIndexDocument(record), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void failed_upload()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("8ad134fa-9045-40af-a0cb-02bc3e868f5a");
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.ResourceLocator = null;
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
                            User = TestUserInfo.TestUser
                        },
                        Resources = new List<Resource> { new Resource{ Path = "x:\\test\\path" } }
                    }
                };
                r.Footer = new Footer();
            });

            var store = new InMemoryDatabaseHelper().Create();
            using (var db = store.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new OpenDataPublishingUploadService(new RecordService(db, new RecordValidator()));
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);

                uploadHelperMock.Setup(x => x.UploadAlternativeResources(record))
                    .Throws(new WebException("test message"));

                uploader.Upload(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.OpenData.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastAttempt.Message.Should().Be("test message");
                updatedRecord.Publication.OpenData.LastSuccess.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void corpulent_upload_with_no_resource_locator()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("e9f1eb92-3fcb-441f-9fdf-520ff52bcf56");
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/metadata-admin",
                        Value = "Corpulent"
                    });
                    m.ResourceLocator = null;
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
                            User = TestUserInfo.TestUser
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

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new OpenDataPublishingUploadService(new RecordService(db, new RecordValidator()));
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);
                uploader.Upload(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.OpenData.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastAttempt.Message.Should().BeNull();
                updatedRecord.Publication.OpenData.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastSuccess.Message.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                updatedRecord.Gemini.ResourceLocator.Should().Be("http://jncc.defra.gov.uk/opendata");
                uploadHelperMock.Verify(x => x.UploadAlternativeResources(record), Times.Never);
                uploadHelperMock.Verify(x => x.UploadDataFile(record.Id, record.Path), Times.Never);
                uploadHelperMock.Verify(x => x.UploadMetadataDocument(record), Times.Once);
                uploadHelperMock.Verify(x => x.UploadWafIndexDocument(record), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void corpulent_upload_with_populated_resource_location()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("bd89e71a-07c4-4ce5-92f6-5121b104b8fe");
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/metadata-admin",
                        Value = "Corpulent"
                    });
                    m.ResourceLocator = "http://www.someexternallinkhere.com";
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
                            User = TestUserInfo.TestUser
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

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new OpenDataPublishingUploadService(new RecordService(db, new RecordValidator()));
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);
                uploader.Upload(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.OpenData.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastAttempt.Message.Should().BeNull();
                updatedRecord.Publication.OpenData.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastSuccess.Message.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                updatedRecord.Gemini.ResourceLocator.Should().Be("http://www.someexternallinkhere.com");
                uploadHelperMock.Verify(x => x.UploadAlternativeResources(record), Times.Never);
                uploadHelperMock.Verify(x => x.UploadDataFile(record.Id, record.Path), Times.Never);
                uploadHelperMock.Verify(x => x.UploadMetadataDocument(record), Times.Once);
                uploadHelperMock.Verify(x => x.UploadWafIndexDocument(record), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void corpulent_upload_with_populated_jncc_resource_location()
        {
            var record = new Record().With(r =>
            {
                r.Id = new Guid("5bc8cd79-7d7f-4c71-9653-cbe82226e174");;
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.Keywords.Add(new MetadataKeyword
                    {
                        Vocab = "http://vocab.jncc.gov.uk/metadata-admin",
                        Value = "Corpulent"
                    });
                    m.ResourceLocator = "http://data.jncc.gov.uk/data/filename";
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
                            User = TestUserInfo.TestUser
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

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new OpenDataPublishingUploadService(new RecordService(db, new RecordValidator()));
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);
                uploader.Upload(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.OpenData.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastAttempt.Message.Should().BeNull();
                updatedRecord.Publication.OpenData.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastSuccess.Message.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                updatedRecord.Gemini.ResourceLocator.Should().Be("http://data.jncc.gov.uk/data/filename");
                uploadHelperMock.Verify(x => x.UploadAlternativeResources(record), Times.Never);
                uploadHelperMock.Verify(x => x.UploadDataFile(record.Id, record.Path), Times.Never);
                uploadHelperMock.Verify(x => x.UploadMetadataDocument(record), Times.Once);
                uploadHelperMock.Verify(x => x.UploadWafIndexDocument(record), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void record_not_corpulent_with_populated_resource_locator()
        {
            var recordId = new Guid("88399fba-b6f5-4e0a-b1d1-fc0668ac7515");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.ResourceLocator = "http://www.someexternallinkhere.com";
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
                            User = TestUserInfo.TestUser
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

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new OpenDataPublishingUploadService(new RecordService(db, new RecordValidator()));
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);

                uploadHelperMock.Setup(x => x.GetHttpRootUrl()).Returns("http://data.jncc.gov.uk");

                uploader.Upload(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.OpenData.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastAttempt.Message.Should().BeNull();
                updatedRecord.Publication.OpenData.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastSuccess.Message.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                updatedRecord.Gemini.ResourceLocator.Should().Be("http://www.someexternallinkhere.com");
                uploadHelperMock.Verify(x => x.UploadAlternativeResources(record), Times.Never);
                uploadHelperMock.Verify(x => x.UploadDataFile(record.Id, record.Path), Times.Never);
                uploadHelperMock.Verify(x => x.UploadMetadataDocument(record), Times.Once);
                uploadHelperMock.Verify(x => x.UploadWafIndexDocument(record), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }
    }
}
