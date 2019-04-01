using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Robot.Publishing;
using Catalogue.Robot.Publishing.Data;
using Catalogue.Robot.Publishing.Gov;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Utilities.Clone;
using Catalogue.Utilities.Text;
using Catalogue.Utilities.Time;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Catalogue.Data.Query;
using Catalogue.Gemini.Model;
using PublicationAttempt = Catalogue.Data.Model.PublicationAttempt;

namespace Catalogue.Tests.Slow.Catalogue.Data.Write
{
    public class publishing_service_specs : CleanDbTest
    {
        private static string HUB_URL_ROOT = "http://hub.jncc.gov.uk/assets/";

        private Env env;

        [OneTimeSetUp]
        public void Init()
        {
            env = new Env(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "TestResources\\.env.test");
        }

        [Test]
        public void successful_publishing_with_various_resources(
            [Values("publication", "dataset", "nonGeographicDataset", "service")] string resourceType)
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
                },
                new List<Resource>
                {
                    new Resource {Name = "Migrated Resource Locator", Path = @"http://data.jncc.gov.uk/data/record-guid-here-filename.txt", PublishedUrl = @"http://data.jncc.gov.uk/data/record-guid-here-filename.txt"}
                },
                new List<Resource>
                {
                    new Resource {Name = "Updated file resource", Path = @"x:\test\file2.txt", PublishedUrl = @"http://data.jncc.gov.uk/data/record-guid-here-file.txt"}
                }
            };

            foreach (var resources in testResources)
            {
                TestSuccessfulHubAndGovPublishing(resourceType, resources);
                TestSuccessfulHubPublishing(resourceType, resources);
                TestSuccessfulGovPublishing(resourceType, resources);
            }
        }

        private void TestSuccessfulHubAndGovPublishing(string resourceType, List<Resource> resources)
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
                    Assessment = new AssessmentInfo { Completed = true },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo { Resources = resources },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

                uploader.PublishRecords(new List<Record> { record });
                var updatedRecord = db.Load<Record>(record.Id);
                
                DataPublishedSuccessfully(updatedRecord, testTime);
                HubPublishedSuccessfully(updatedRecord, testTime);
                GovPublishedSuccessfully(updatedRecord, testTime);
                ResourcesUpdatedCorrectly(recordId, record.Publication.Data.Resources, updatedRecord.Publication.Data.Resources);

                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);

                var fileCount = CountFileResources(record.Publication.Data.Resources);
                dataUploaderMock.Verify(x => x.UploadDataFile(Helpers.RemoveCollection(record.Id), It.IsAny<string>()), Times.Exactly(fileCount));
                hubServiceMock.Verify(x => x.Save(record), Times.Once);
                record.Publication.Target.Hub.Url =
                    "http://hub.jncc.gov.uk/assets/" + Helpers.RemoveCollection(recordId);
                hubServiceMock.Verify(x => x.Index(record), Times.Once);
                metadataUploaderMock.Verify(x => x.UploadGeminiXml(Helpers.RemoveCollectionFromId(record)), Times.Once);
                metadataUploaderMock.Verify(x => x.UpdateDguIndex(Helpers.RemoveCollectionFromId(record)), Times.Once);
                redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Exactly(2));

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        private void TestSuccessfulHubPublishing(string resourceType, List<Resource> resources)
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
                    Assessment = new AssessmentInfo { Completed = true },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo { Resources = resources },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

                uploader.PublishRecords(new List<Record> { record });
                var updatedRecord = db.Load<Record>(record.Id);

                DataPublishedSuccessfully(updatedRecord, testTime);
                HubPublishedSuccessfully(updatedRecord, testTime);
                updatedRecord.Publication.Target.Gov.Should().BeNull();
                ResourcesUpdatedCorrectly(recordId, record.Publication.Data.Resources, updatedRecord.Publication.Data.Resources);

                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                var fileCount = CountFileResources(record.Publication.Data.Resources);

                dataUploaderMock.Verify(x => x.UploadDataFile(Helpers.RemoveCollection(record.Id), It.IsAny<string>()), Times.Exactly(fileCount));
                hubServiceMock.Verify(x => x.Save(record), Times.Once);
                record.Publication.Target.Hub.Url =
                    "http://hub.jncc.gov.uk/assets/" + Helpers.RemoveCollection(recordId);
                hubServiceMock.Verify(x => x.Index(record), Times.Once);
                metadataUploaderMock.Verify(x => x.UploadGeminiXml(It.IsAny<Record>()), Times.Never);
                metadataUploaderMock.Verify(x => x.UpdateDguIndex(It.IsAny<Record>()), Times.Never);
                redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        private void TestSuccessfulGovPublishing(string resourceType, List<Resource> resources)
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
                    Assessment = new AssessmentInfo { Completed = true },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo { Resources = resources },
                    Target = new TargetInfo
                    {
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

                uploader.PublishRecords(new List<Record> { record });
                var updatedRecord = db.Load<Record>(record.Id);

                DataPublishedSuccessfully(updatedRecord, testTime);
                updatedRecord.Publication.Target.Hub.Should().BeNull();
                GovPublishedSuccessfully(updatedRecord, testTime);
                ResourcesUpdatedCorrectly(recordId, record.Publication.Data.Resources, updatedRecord.Publication.Data.Resources);

                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                var fileCount = CountFileResources(record.Publication.Data.Resources);

                dataUploaderMock.Verify(x => x.UploadDataFile(Helpers.RemoveCollection(record.Id), It.IsAny<string>()), Times.Exactly(fileCount));
                hubServiceMock.Verify(x => x.Save(It.IsAny<Record>()), Times.Never);
                hubServiceMock.Verify(x => x.Index(It.IsAny<Record>()), Times.Never);
                metadataUploaderMock.Verify(x => x.UploadGeminiXml(record), Times.Once);
                metadataUploaderMock.Verify(x => x.UpdateDguIndex(record), Times.Once);
                redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void previously_published_to_hub_and_now_to_gov()
        {
            var recordId = Guid.NewGuid().ToString();
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\working\folder";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo { Completed = true },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo { Resources = new List<Resource>{new Resource { Name = "File resource", Path = "x:\\test\\path.txt" }} },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = false, //previously published here
                            Url = "http://hub.jncc.gov.uk/assets/record-guid",
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 08, 17, 12, 0, 0)
                            }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true // now going to be published here
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

                uploader.PublishRecords(new List<Record> { record });
                var updatedRecord = db.Load<Record>(record.Id);

                DataPublishedSuccessfully(updatedRecord, testTime);
                updatedRecord.Publication.Data.Resources.Should().Contain(r => r.Name.Equals("File resource"));
                updatedRecord.Publication.Target.Hub.LastSuccess.DateUtc.Should().Be(new DateTime(2017, 08, 17, 12, 0, 0));
                updatedRecord.Publication.Target.Hub.Url.Should().Be("http://hub.jncc.gov.uk/assets/record-guid");
                GovPublishedSuccessfully(updatedRecord, testTime);
                ResourcesUpdatedCorrectly(recordId, record.Publication.Data.Resources, updatedRecord.Publication.Data.Resources);

                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);

                var fileCount = CountFileResources(record.Publication.Data.Resources);
                dataUploaderMock.Verify(x => x.UploadDataFile(Helpers.RemoveCollection(record.Id), It.IsAny<string>()), Times.Exactly(fileCount));
                hubServiceMock.Verify(x => x.Save(It.IsAny<Record>()), Times.Never);
                hubServiceMock.Verify(x => x.Index(It.IsAny<Record>()), Times.Never);
                metadataUploaderMock.Verify(x => x.UploadGeminiXml(record), Times.Once);
                metadataUploaderMock.Verify(x => x.UpdateDguIndex(record), Times.Once);
                redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void previously_published_to_gov_and_now_to_hub()
        {
            var recordId = Guid.NewGuid().ToString();
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\working\folder";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example();
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo { Completed = true },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo { Resources = new List<Resource> { new Resource { Name = "File resource", Path = "x:\\test\\path.txt" } } },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true // now going to publish here
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = false, // previously published here
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 08, 17, 12, 0, 0)
                            }
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

                uploader.PublishRecords(new List<Record> { record });
                var updatedRecord = db.Load<Record>(record.Id);

                DataPublishedSuccessfully(updatedRecord, testTime);
                updatedRecord.Publication.Data.Resources.Should().Contain(r => r.Name.Equals("File resource"));
                HubPublishedSuccessfully(updatedRecord, testTime);
                updatedRecord.Publication.Target.Gov.LastSuccess.DateUtc.Should().Be(new DateTime(2017, 08, 17, 12, 0, 0));
                ResourcesUpdatedCorrectly(recordId, record.Publication.Data.Resources, updatedRecord.Publication.Data.Resources);

                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                
                var fileCount = CountFileResources(record.Publication.Data.Resources);
                dataUploaderMock.Verify(x => x.UploadDataFile(Helpers.RemoveCollection(record.Id), It.IsAny<string>()), Times.Exactly(fileCount));
                hubServiceMock.Verify(x => x.Save(record), Times.Once);
                hubServiceMock.Verify(x => x.Index(record), Times.Once);
                metadataUploaderMock.Verify(x => x.UploadGeminiXml(Helpers.RemoveCollectionFromId(record)), Times.Never);
                metadataUploaderMock.Verify(x => x.UpdateDguIndex(Helpers.RemoveCollectionFromId(record)), Times.Never);
                redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void publish_fails_at_data_upload(
            [Values("publication", "dataset", "nonGeographicDataset", "service")] string resourceType)
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
                    Assessment = new AssessmentInfo
                    {
                        Completed = true
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "Some resource", Path = "x:\\test\\path.txt" } }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                dataUploaderMock.Setup(x => x.UploadDataFile(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("test message"));

                uploader.PublishRecords(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.Data.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Data.LastAttempt.Message.Should().Be("test message");
                updatedRecord.Publication.Data.LastSuccess.Should().BeNull();
                updatedRecord.Publication.Target.Hub.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Target.Hub.LastSuccess.Should().BeNull();
                updatedRecord.Publication.Target.Gov.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Target.Hub.LastSuccess.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void publish_fails_at_second_data_upload(
            [Values("publication", "dataset", "nonGeographicDataset", "service")] string resourceType)
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
                    Assessment = new AssessmentInfo
                    {
                        Completed = true
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource>
                        {
                            new Resource { Name = "File resource", Path = "x:\\a\\test\\path.txt" },
                            new Resource { Name = "Another file resource", Path = "x:\\another\\test\\path.txt" },
                            new Resource { Name = "Web resource", Path = "http://a.web.resource" }
                        }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                dataUploaderMock.Setup(x => x.UploadDataFile(It.IsAny<string>(), "x:\\another\\test\\path.txt")).Throws(new WebException("test message"));

                uploader.PublishRecords(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.Data.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Data.LastAttempt.Message.Should().Be("test message");
                updatedRecord.Publication.Data.LastSuccess.Should().BeNull();
                foreach (var resource in updatedRecord.Publication.Data.Resources)
                {
                    resource.PublishedUrl.Should().BeNullOrEmpty();
                }
                updatedRecord.Publication.Target.Hub.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Target.Hub.LastSuccess.Should().BeNull();
                updatedRecord.Publication.Target.Gov.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Target.Gov.LastSuccess.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void publish_fails_at_hub_upload(
            [Values("publication", "dataset", "nonGeographicDataset", "service")] string resourceType)
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
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedByUser = TestUserInfo.TestUser
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "Some resource", Path = "x:\\test\\path" } }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                hubServiceMock.Setup(x => x.Save(It.IsAny<Record>())).Throws(new Exception("test message"));
                redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

                uploader.PublishRecords(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.Data.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Data.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Hub.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Hub.LastAttempt.Message.Should().Be("test message");
                updatedRecord.Publication.Target.Hub.LastSuccess.Should().BeNull();
                updatedRecord.Publication.Target.Gov.LastAttempt.Should().BeNull();
                updatedRecord.Publication.Target.Gov.LastSuccess.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Once);

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void publish_fails_at_hub_index(
            [Values("publication", "dataset", "nonGeographicDataset", "service")] string resourceType)
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
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedByUser = TestUserInfo.TestUser
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "Some resource", Path = "x:\\test\\path" } }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                hubServiceMock.Setup(x => x.Index(It.IsAny<Record>())).Throws(new Exception("test message"));
                redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

                uploader.PublishRecords(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.Data.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Data.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Hub.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Hub.LastAttempt.Message.Should().BeNull();
                updatedRecord.Publication.Target.Hub.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Gov.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Gov.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Exactly(2));

                Clock.CurrentUtcDateTimeGetter = currentTime;
            }
        }

        [Test]
        public void publish_fails_at_dgu_upload(
            [Values("publication", "dataset", "nonGeographicDataset", "service")] string resourceType)
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
                    Assessment = new AssessmentInfo
                    {
                        Completed = true
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "Some resource", Path = "x:\\test\\path" } }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
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

                var vocabQueryerMock = new Mock<IVocabQueryer>();
                var uploadService = new PublishingUploadRecordService(db, new RecordValidator(vocabQueryerMock.Object));
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IGovService>();
                var hubServiceMock = new Mock<IHubService>();
                var redactorMock = new Mock<IRecordRedactor>();
                var uploader = new RobotPublisher(env, db, redactorMock.Object, uploadService, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);

                var recordWithoutCollection = Helpers.RemoveCollectionFromId(record);
                vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
                metadataUploaderMock.Setup(x => x.UploadGeminiXml(recordWithoutCollection)).Throws(new Exception("test message"));
                hubServiceMock.Setup(x => x.Save(It.IsAny<Record>()));
                redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

                uploader.PublishRecords(new List<Record> { record });

                var updatedRecord = db.Load<Record>(record.Id);
                updatedRecord.Publication.Data.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Data.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Hub.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Hub.LastSuccess.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Gov.LastAttempt.DateUtc.Should().Be(testTime);
                updatedRecord.Publication.Target.Gov.LastAttempt.Message.Should().Be("test message");
                updatedRecord.Publication.Target.Gov.LastSuccess.Should().BeNull();
                updatedRecord.Gemini.MetadataDate.Should().Be(testTime);
                redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Exactly(2));

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
            var hub = record.Publication.Target.Hub;
            hub.LastAttempt.DateUtc.Should().Be(timestamp);
            hub.LastAttempt.Message.Should().BeNull();
            hub.LastSuccess.DateUtc.Should().Be(timestamp);
            hub.LastSuccess.Message.Should().BeNull();
            hub.Url.Should().Be(HUB_URL_ROOT + Helpers.RemoveCollection(record.Id));
        }

        private void GovPublishedSuccessfully(Record record, DateTime timestamp)
        {
            var gov = record.Publication.Target.Gov;
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
                    string dataPath = WebificationUtility.GetUnrootedDataPath(recordId, originalResource.Path);
                    updatedResource.PublishedUrl.Should().Be("http://data.jncc.gov.uk/" + dataPath);
                }
                else
                {
                    updatedResource.PublishedUrl.Should().BeNullOrEmpty();
                }
            }
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
