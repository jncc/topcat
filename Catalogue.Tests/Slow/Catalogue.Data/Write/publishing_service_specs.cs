using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Time;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Catalogue.Utilities.Text;

namespace Catalogue.Tests.Slow.Catalogue.Data.Write
{
    public class publishing_service_specs : CleanDbTest
    {
        private static string HUB_URL_ROOT = "http://hub.jncc.gov.uk/asset/";

        [Test]
        public void successful_open_data_publishing(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType)
        {
            var testResources = new List<List<Resource>>
            {
                new List<Resource>
                {
                    new Resource {Name = "Web resource", Path = @"http://a.web.resource"}
                },
                new List<Resource>
                {
                    new Resource {Name = "Web resource", Path = @"http://a.web.resource"},
                    new Resource {Name = "Another web resource", Path = @"http://another.web.resource"}
                },
                new List<Resource>
                {
                    new Resource {Name = "File resource", Path = "x:\\test\\path.txt"}
                },
                new List<Resource>
                {
                    new Resource {Name = "File resource", Path = "x:\\test\\path.txt"},
                    new Resource {Name = "Another file resource", Path = "x:\\another\\test\\path.txt"}
                },
                new List<Resource>
                {
                    new Resource {Name = "Web resource", Path = @"http://a.web.resource"},
                    new Resource {Name = "File resource", Path = "x:\\test\\path.txt"},
                    new Resource {Name = "Another file resource", Path = "x:\\another\\test\\path.txt"}
                }
            };

            foreach (var resources in testResources)
            {
                TestSuccessfulOpenDataPublishing(resourceType, resources);
            }
        }

        private void TestSuccessfulOpenDataPublishing(string resourceType, List<Resource> resources)
        {
            var recordId = Guid.NewGuid().ToString();
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\working\folder";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m => m.ResourceType = resourceType);
                r.Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo { Resources = resources },
                    Hub = new HubPublicationInfo(),
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true,
                        Assessment = new OpenDataAssessmentInfo { Completed = true },
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
                var hubServiceMock = new Mock<IHubService>();
                var uploader = new RobotPublisher(db, uploadService, uploadHelperMock.Object, hubServiceMock.Object);
                uploadHelperMock.Setup(x => x.GetHttpRootUrl()).Returns("http://data.jncc.gov.uk");

                uploader.PublishRecords(new List<Record> { record });
                var updatedRecord = db.Load<Record>(record.Id);

                DataPublishedSuccessfully(record, testTime);
                HubPublishedSuccessfully(record, testTime);
                GovPublishedSuccessfully(record, testTime);
                ResourcesUpdatedCorrectly(recordId, record.Publication.Data.Resources, updatedRecord.Publication.Data.Resources);

                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                CheckMethodInvocations(record, uploadHelperMock, hubServiceMock);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void open_data_record_fails_at_only_data_upload(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType)
        {
            var recordId = Guid.NewGuid().ToString();
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m => m.ResourceType = resourceType);
                r.Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "Some resource", Path = "x:\\test\\path.txt" } }
                    },
                    Hub = new HubPublicationInfo(),
                    Gov = new GovPublicationInfo
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

            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new PublishingUploadRecordService(db, new RecordValidator());
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var hubServiceMock = new Mock<IHubService>();
                var uploader = new RobotPublisher(db, uploadService, uploadHelperMock.Object, hubServiceMock.Object);

                uploadHelperMock.Setup(x => x.UploadDataFile(It.IsAny<string>(), It.IsAny<string>())).Throws(new WebException("test message"));

                uploader.PublishRecords(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.Data.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Data.LastAttempt.Message.Should().Be("test message");
                updatedRecord.Publication.Data.LastSuccess.Should().BeNull();
                updatedRecord.Publication.Hub.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Hub.LastSuccess.Should().BeNull();
                updatedRecord.Publication.Gov.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Gov.LastSuccess.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void open_data_record_fails_at_second_data_upload(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType)
        {
            var recordId = Guid.NewGuid().ToString();
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m => m.ResourceType = resourceType);
                r.Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource>
                        {
                            new Resource { Name = "File resource", Path = "x:\\a\\test\\path.txt" },
                            new Resource { Name = "Another file resource", Path = "x:\\another\\test\\path.txt" },
                            new Resource { Name = "Web resource", Path = "http://a.web.resource" }
                        }
                    },
                    Hub = new HubPublicationInfo(),
                    Gov = new GovPublicationInfo
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

            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new PublishingUploadRecordService(db, new RecordValidator());
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var hubServiceMock = new Mock<IHubService>();
                var uploader = new RobotPublisher(db, uploadService, uploadHelperMock.Object, hubServiceMock.Object);

                uploadHelperMock.Setup(x => x.UploadDataFile(It.IsAny<string>(), "x:\\another\\test\\path.txt")).Throws(new WebException("test message"));

                uploader.PublishRecords(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.Data.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Data.LastAttempt.Message.Should().Be("test message");
                updatedRecord.Publication.Data.LastSuccess.Should().BeNull();
                foreach (var resource in updatedRecord.Publication.Data.Resources)
                {
                    resource.PublishedUrl.Should().BeNullOrEmpty();
                }
                updatedRecord.Publication.Hub.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Hub.LastSuccess.Should().BeNull();
                updatedRecord.Publication.Gov.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Gov.LastSuccess.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void open_data_record_fails_at_datahub_upload(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType)
        {
            var recordId = Guid.NewGuid().ToString();
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m => m.ResourceType = resourceType);
                r.Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "Some resource", Path = "x:\\test\\path" } }
                    },
                    Hub = new HubPublicationInfo(),
                    Gov = new GovPublicationInfo
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

            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new PublishingUploadRecordService(db, new RecordValidator());
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var hubServiceMock = new Mock<IHubService>();
                var uploader = new RobotPublisher(db, uploadService, uploadHelperMock.Object, hubServiceMock.Object);
                hubServiceMock.Setup(x => x.Upsert(It.IsAny<Record>())).Throws(new WebException("test message"));

                uploader.PublishRecords(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.Data.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Data.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Hub.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Hub.LastAttempt.Message.Should().Be("test message");
                updatedRecord.Publication.Hub.LastSuccess.Should().BeNull();
                updatedRecord.Publication.Gov.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Gov.LastSuccess.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void open_data_record_fails_at_dgu_upload(
            [Values("dataset", "nonGeographicDataset", "service")] string resourceType)
        {
            var recordId = Guid.NewGuid().ToString();
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\upload\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m => m.ResourceType = resourceType);
                r.Publication = new PublicationInfo
                {
                    Data = new DataPublicationInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "Some resource", Path = "x:\\test\\path" } }
                    },
                    Hub = new HubPublicationInfo(),
                    Gov = new GovPublicationInfo
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
            
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();

                var currentTime = Clock.CurrentUtcDateTimeGetter;
                var testTime = new DateTime(2017, 08, 18, 12, 0, 0);
                Clock.CurrentUtcDateTimeGetter = () => testTime;

                var uploadService = new PublishingUploadRecordService(db, new RecordValidator());
                var uploadHelperMock = new Mock<IOpenDataUploadHelper>();
                var hubServiceMock = new Mock<IHubService>();
                var uploader = new RobotPublisher(db, uploadService, uploadHelperMock.Object, hubServiceMock.Object);

                var recordWithoutCollection = Helpers.RemoveCollectionFromId(record);
                uploadHelperMock.Setup(x => x.UploadMetadataDocument(recordWithoutCollection)).Throws(new WebException("test message"));
                hubServiceMock.Setup(x => x.Upsert(It.IsAny<Record>()));

                uploader.PublishRecords(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.Data.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Data.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Hub.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Hub.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Gov.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Gov.LastAttempt.Message.Should().Be("test message");
                updatedRecord.Publication.Gov.LastSuccess.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        private void DataPublishedSuccessfully(Record record, DateTime timestamp)
        {
            var data = record.Publication.Data;
            data.LastAttempt.DateUtc.Should().Be(timestamp);
            data.LastAttempt.Message.Should().BeNull();
            data.LastSuccess.DateUtc.Should().Be(timestamp);
            data.LastSuccess.Message.Should().BeNull();
        }

        private void HubPublishedSuccessfully(Record record, DateTime timestamp)
        {
            var hub = record.Publication.Hub;
            hub.LastAttempt.DateUtc.Should().Be(timestamp);
            hub.LastAttempt.Message.Should().BeNull();
            hub.LastSuccess.DateUtc.Should().Be(timestamp);
            hub.LastSuccess.Message.Should().BeNull();
            hub.Url.Should().Be(HUB_URL_ROOT + Helpers.RemoveCollection(record.Id));
        }

        private void GovPublishedSuccessfully(Record record, DateTime timestamp)
        {
            var gov = record.Publication.Gov;
            gov.LastAttempt.DateUtc.Should().Be(timestamp);
            gov.LastAttempt.Message.Should().BeNull();
            gov.LastSuccess.DateUtc.Should().Be(timestamp);
            gov.LastSuccess.Message.Should().BeNull();
        }

        private void ResourcesUpdatedCorrectly(string recordId, List<Resource> originalResources, List<Resource> updatedResources)
        {
            originalResources.Count.Should().Be(updatedResources.Count);
            foreach (var originalResource in originalResources)
            {
                updatedResources.Count(r => r.Path.Equals(originalResource.Path)).Should().Be(1);
                var updatedResource = updatedResources.Find(x => x.Path.Equals(originalResource.Path));
                if (IsFileResource(updatedResource))
                {
                    string filename = WebificationUtility.GetUnrootedDataPath(recordId, originalResource.Path);
                    updatedResource.PublishedUrl.Should().Be("http://data.jncc.gov.uk/" + filename);
                }
                else
                {
                    updatedResource.PublishedUrl.Should().BeNullOrEmpty();
                }
            }
        }

        private void CheckMethodInvocations(Record record, Mock<IOpenDataUploadHelper> uploadHelperMock, Mock<IHubService> hubServiceMock)
        {
            var fileCount = CountFileResources(record.Publication.Data.Resources);

            uploadHelperMock.Verify(x => x.UploadDataFile(Helpers.RemoveCollection(record.Id), It.IsAny<string>()), Times.Exactly(fileCount));
            hubServiceMock.Verify(x => x.Upsert(record), Times.Once);
            hubServiceMock.Verify(x => x.Index(record), Times.Once);
            uploadHelperMock.Verify(x => x.UploadMetadataDocument(record), Times.Once);
            uploadHelperMock.Verify(x => x.UploadWafIndexDocument(record), Times.Once);
        }

        private int CountFileResources(List<Resource> resources)
        {
            var files = 0;
            
            foreach (var resource in resources) {
                if (IsFileResource(resource))
                {
                    files++;
                }
            }

            return files;
        }

        private bool IsFileResource(Resource resource)
        {
            var isFilePath = false;
            if (Uri.TryCreate(resource.Path, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    isFilePath = true;
                }
            }

            return isFilePath;
        }
    }
}
