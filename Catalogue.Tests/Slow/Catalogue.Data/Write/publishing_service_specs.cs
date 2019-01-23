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
using System.Linq;
using System.Net;
using Catalogue.Data;
using Catalogue.Robot.Publishing.OpenData;
using Moq;

namespace Catalogue.Tests.Slow.Catalogue.Data.Write
{
    public class publishing_service_specs : CleanDbTest
    {
        private static string DATAHUB_ROOT = "http://datahub.jncc.gov.uk/";

        [Test]
        public void successful_publish_of_open_data_record()
        {
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("eb189a2f-ebce-4232-8dc6-1ad486cacf21");
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Publishable = true,
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\upload\test.txt" } },
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
            
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new PublishingUploadRecordService(db, new RecordValidator());
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);

                uploadHelperMock.Setup(x => x.GetHttpRootUrl()).Returns("http://data.jncc.gov.uk");

                uploader.Upload(new List<Record>{record});

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.OpenData.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastAttempt.Message.Should().BeNull();
                updatedRecord.Publication.OpenData.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.OpenData.LastSuccess.Message.Should().BeNull();
                updatedRecord.Publication.OpenData.Resources.First(r => r.Name.Equals("File resource")).PublishedUrl.Should().Be("http://data.jncc.gov.uk/data/eb189a2f-ebce-4232-8dc6-1ad486cacf21-test.txt");
                updatedRecord.Publication.OpenData.Resources.First(r => r.Name.Equals("File resource")).Path.Should().Be(@"X:\path\to\upload\test.txt");
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                uploadHelperMock.Verify(x => x.UploadDataFile(Helpers.RemoveCollection(record.Id), @"X:\path\to\upload\test.txt"), Times.Once);
                uploadHelperMock.Verify(x => x.UploadMetadataDocument(record), Times.Once);
                uploadHelperMock.Verify(x => x.UploadWafIndexDocument(record), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void open_data_record_with_one_file_resource(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType)
        {
            var recordId = "eb189a2f-ebce-4232-8dc6-1ad486cacf21";
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example()
                    .With(m => m.ResourceType = resourceType);
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Publishable = true,
                        Assessment = new OpenDataAssessmentInfo
                        {
                            Completed = true
                        },
                        SignOff = new OpenDataSignOffInfo
                        {
                            DateUtc = new DateTime(2017, 08, 02),
                            User = TestUserInfo.TestUser
                        },
                        Resources = new List<Resource>{ new Resource{ Name = "Some resource", Path = "x:\\test\\path.txt" } }
                    }
                };
                r.Footer = new Footer();
            });
            
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new PublishingUploadRecordService(db, new RecordValidator());
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);

                uploadHelperMock.Setup(x => x.GetHttpRootUrl()).Returns("http://data.jncc.gov.uk");

                uploader.Upload(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                var openData = updatedRecord.Publication.OpenData;
                var datahub = updatedRecord.Publication.Datahub;
                openData.LastAttempt.DateUtc.Should().Be(testTime);
                openData.LastAttempt.Message.Should().BeNull();
                openData.LastSuccess.DateUtc.Should().Be(testTime);
                openData.LastSuccess.Message.Should().BeNull();
                openData.Resources.First().PublishedUrl.Should().Be("http://data.jncc.gov.uk/data/"+ recordId + "-path.txt");
                datahub.Url.Should().Be(DATAHUB_ROOT + recordId);
                datahub.LastAttempt.DateUtc.Should().Be(testTime);
                datahub.LastAttempt.Message.Should().BeNull();
                datahub.LastSuccess.DateUtc.Should().Be(testTime);
                datahub.LastSuccess.Message.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                uploadHelperMock.Verify(x => x.UploadDataFile(Helpers.RemoveCollection(record.Id), record.Path), Times.Once);
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
                r.Id = Helpers.AddCollection("8ad134fa-9045-40af-a0cb-02bc3e868f5a");
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
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
                        Resources = new List<Resource> { new Resource{ Name = "Some resource", Path = "x:\\test\\path" } }
                    }
                };
                r.Footer = new Footer();
            });
            
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new PublishingUploadRecordService(db, new RecordValidator());
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var uploader = new RobotUploader(db, uploadService, uploadHelperMock.Object);

                var recordWithoutCollection = Helpers.RemoveCollectionFromId(record);
                uploadHelperMock.Setup(x => x.UploadMetadataDocument(recordWithoutCollection)).Throws(new WebException("test message"));

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
        public void record_not_corpulent_with_populated_resource_locator()
        {
            var recordId = Helpers.AddCollection("88399fba-b6f5-4e0a-b1d1-fc0668ac7515");
            var record = new Record().With(r =>
            {
                r.Id = recordId;
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.ResourceType = "dataset";
                });
                r.Publication = new PublicationInfo
                {
                    OpenData = new OpenDataPublicationInfo
                    {
                        Publishable = true,
                        Resources = new List<Resource> { new Resource
                        {
                            Name = "External link",
                            Path = "http://www.someexternallinkhere.com"
                        } },
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
            
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new PublishingUploadRecordService(db, new RecordValidator());
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
                updatedRecord.Publication.OpenData.Resources.First().Path.Should().Be("http://www.someexternallinkhere.com");
                updatedRecord.Publication.OpenData.Resources.First().PublishedUrl.Should().BeNullOrEmpty();
                uploadHelperMock.Verify(x => x.UploadDataFile(record.Id, record.Path), Times.Never);
                uploadHelperMock.Verify(x => x.UploadMetadataDocument(record), Times.Once);
                uploadHelperMock.Verify(x => x.UploadWafIndexDocument(record), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }
    }
}
