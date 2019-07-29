using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Robot.Publishing;
using Catalogue.Robot.Publishing.Data;
using Catalogue.Robot.Publishing.Gov;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Utilities.Clone;
using Moq;
using NUnit.Framework;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using PublicationAttempt = Catalogue.Data.Model.PublicationAttempt;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    public class robot_publisher_publish_record_specs
    {
        private Env env;

        [OneTimeSetUp]
        public void Init()
        {
            env = new Env(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "TestResources\\.env.test");
        }

        [Test]
        public void data_cleanup_for_successful_hub_and_gov_publish()
        {
            var recordId = Guid.NewGuid().ToString();
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\working\folder";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m => m.ResourceType = "dataset");
                r.Resources = new List<Resource>
                {
                    new Resource {Name = "File resource", Path = "x:\\test\\path.txt"}
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo { Completed = true },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
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

            var db = new Mock<IDocumentSession>();
            var vocabQueryerMock = new Mock<IVocabQueryer>();
            var uploadService = new Mock<IPublishingUploadRecordService>();
            var dataUploaderMock = new Mock<IDataService>();
            var metadataUploaderMock = new Mock<IGovService>();
            var hubServiceMock = new Mock<IHubService>();
            var redactorMock = new Mock<IRecordRedactor>();
            var uploader = new RobotPublisher(env, db.Object, redactorMock.Object, uploadService.Object, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
            vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
            redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

            uploader.PublishRecord(record);
            
            dataUploaderMock.Verify(x => x.CreateDataRollback(It.IsAny<string>()), Times.Once);
            dataUploaderMock.Verify(x => x.UploadDataFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            hubServiceMock.Verify(x => x.Publish(It.IsAny<Record>()), Times.Once);
            hubServiceMock.Verify(x => x.Index(It.IsAny<Record>()), Times.Once);
            metadataUploaderMock.Verify(x => x.UploadGeminiXml(It.IsAny<Record>()), Times.Once);
            metadataUploaderMock.Verify(x => x.UpdateDguIndex(It.IsAny<Record>()), Times.Once);
            uploadService.Verify(x => x.UpdateGovPublishSuccess(It.IsAny<Record>(), It.IsAny<PublicationAttempt>()), Times.Once);
            redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Exactly(2));
            dataUploaderMock.Verify(x => x.Rollback(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void data_rollback_for_unsuccessful_hub_and_gov_publish()
        {
            var recordId = Guid.NewGuid().ToString();
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(recordId);
                r.Path = @"X:\path\to\working\folder";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m => m.ResourceType = "dataset");
                r.Resources = new List<Resource>
                {
                    new Resource {Name = "File resource", Path = "x:\\test\\path.txt"}
                };
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo { Completed = true },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 08, 02),
                        User = TestUserInfo.TestUser
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

            var db = new Mock<IDocumentSession>();
            var vocabQueryerMock = new Mock<IVocabQueryer>();
            var uploadService = new Mock<IPublishingUploadRecordService>();
            var dataServiceMock = new Mock<IDataService>();
            var metadataUploaderMock = new Mock<IGovService>();
            var hubServiceMock = new Mock<IHubService>();
            var redactorMock = new Mock<IRecordRedactor>();
            var uploader = new RobotPublisher(env, db.Object, redactorMock.Object, uploadService.Object, dataServiceMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);
            vocabQueryerMock.Setup(x => x.GetVocab(It.IsAny<string>())).Returns(new Vocabulary());
            hubServiceMock.Setup(x => x.Publish(It.IsAny<Record>())).Throws(new Exception("test message"));
            redactorMock.Setup(x => x.RedactRecord(It.IsAny<Record>())).Returns(record);

            uploader.PublishRecord(record);

            dataServiceMock.Verify(x => x.CreateDataRollback(It.IsAny<string>()), Times.Once);
            dataServiceMock.Verify(x => x.UploadDataFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            hubServiceMock.Verify(x => x.Publish(It.IsAny<Record>()), Times.Once);
            hubServiceMock.Verify(x => x.Index(It.IsAny<Record>()), Times.Never);
            metadataUploaderMock.Verify(x => x.UploadGeminiXml(It.IsAny<Record>()), Times.Never);
            metadataUploaderMock.Verify(x => x.UpdateDguIndex(It.IsAny<Record>()), Times.Never);
            uploadService.Verify(x => x.UpdateGovPublishSuccess(It.IsAny<Record>(), It.IsAny<PublicationAttempt>()), Times.Never);
            redactorMock.Verify(x => x.RedactRecord(It.IsAny<Record>()), Times.Exactly(1));
            dataServiceMock.Verify(x => x.RemoveRollbackFiles(It.IsAny<string>()), Times.Never);
            dataServiceMock.Verify(x => x.Rollback(It.IsAny<string>()), Times.Once);
        }
    }
}
