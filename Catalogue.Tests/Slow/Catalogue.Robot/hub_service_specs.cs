using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Robot.Publishing;
using Catalogue.Robot.Publishing.Client;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Utilities.Clone;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.Model;
using Amazon.S3.Model;
using Catalogue.Gemini.Templates;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    public class hub_service_specs
    {
        private Env env;

        [OneTimeSetUp]
        public void Init()
        {
            env = new Env(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "TestResources\\.env.test");
        }

        [Test]
        public void record_with_one_pdf_resource_test()
        {
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("0545c14b-e7fd-472d-8575-5bb75034945f");
                r.Gemini = Library.Example();
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "A pdf resource",
                        Path = "C:\\work\\test.pdf",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                    }
                };
            });

            var lambdaClientMock = new Mock<ILambdaClient>();
            var s3ClientMock = new Mock<IS3Client>();
            var fileHelperMock = new Mock<IFileHelper>();
            var hubService = new HubService(env, lambdaClientMock.Object, s3ClientMock.Object, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.IsPdfFile(It.IsAny<string>())).Returns(true);
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(100);
            fileHelperMock.Setup(x => x.GetBase64String(It.IsAny<string>())).Returns("base 64 string");
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot(It.IsAny<string>())).Returns("pdf");
            var s3Response = new PutObjectResponse();
            s3Response.HttpStatusCode = HttpStatusCode.OK;
            s3ClientMock.Setup(x => x.SaveToS3(It.IsAny<string>(), It.IsAny<string>())).Returns(s3Response);
            var lambdaResponse = new InvokeResponse();
            lambdaResponse.StatusCode = 200;
            lambdaClientMock.Setup(x => x.SendToHub(It.IsAny<string>())).Returns(lambdaResponse);

            hubService.Publish(record);

            s3ClientMock.Verify(x => x.SaveToS3(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            lambdaClientMock.Verify(x => x.SendToHub(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void record_with_multiple_pdf_resources_test()
        {
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("0545c14b-e7fd-472d-8575-5bb75034945f");
                r.Gemini = Library.Example();
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "A pdf resource",
                        Path = "C:\\work\\test.pdf",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.pdf"
                    },
                    new Resource
                    {
                        Name = "Another pdf resource",
                        Path = "C:\\work\\test2.pdf",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test2.pdf"
                    }
                };
            });

            var lambdaClientMock = new Mock<ILambdaClient>();
            var s3ClientMock = new Mock<IS3Client>();
            var fileHelperMock = new Mock<IFileHelper>();
            var hubService = new HubService(env, lambdaClientMock.Object, s3ClientMock.Object, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.IsPdfFile(It.IsAny<string>())).Returns(true);
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(100);
            fileHelperMock.Setup(x => x.GetBase64String(It.IsAny<string>())).Returns("base 64 string");
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot(It.IsAny<string>())).Returns("pdf");
            var s3Response = new PutObjectResponse();
            s3Response.HttpStatusCode = HttpStatusCode.OK;
            s3ClientMock.Setup(x => x.SaveToS3(It.IsAny<string>(), It.IsAny<string>())).Returns(s3Response);
            var lambdaResponse = new InvokeResponse();
            lambdaResponse.StatusCode = 200;
            lambdaClientMock.Setup(x => x.SendToHub(It.IsAny<string>())).Returns(lambdaResponse);

            hubService.Publish(record);

            s3ClientMock.Verify(x => x.SaveToS3(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            lambdaClientMock.Verify(x => x.SendToHub(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void record_with_no_pdfs_test()
        {
            var record = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection("0545c14b-e7fd-472d-8575-5bb75034945f");
                r.Gemini = Library.Example();
                r.Resources = new List<Resource>
                {
                    new Resource
                    {
                        Name = "A csv resource",
                        Path = "C:\\work\\test.csv",
                        PublishedUrl = "http://data.jncc.gov.uk/data/0545c14b-e7fd-472d-8575-5bb75034945f/test.csv"
                    }
                };
            });

            var lambdaClientMock = new Mock<ILambdaClient>();
            var s3ClientMock = new Mock<IS3Client>();
            var fileHelperMock = new Mock<IFileHelper>();
            var hubService = new HubService(env, lambdaClientMock.Object, s3ClientMock.Object, fileHelperMock.Object);
            fileHelperMock.Setup(x => x.IsPdfFile(It.IsAny<string>())).Returns(false);
            fileHelperMock.Setup(x => x.GetFileSizeInBytes(It.IsAny<string>())).Returns(100);
            fileHelperMock.Setup(x => x.GetFileExtensionWithoutDot(It.IsAny<string>())).Returns("csv");
            var lambdaResponse = new InvokeResponse();
            lambdaResponse.StatusCode = 200;
            lambdaClientMock.Setup(x => x.SendToHub(It.IsAny<string>())).Returns(lambdaResponse);

            hubService.Publish(record);

            s3ClientMock.Verify(x => x.SaveToS3(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            lambdaClientMock.Verify(x => x.SendToHub(It.IsAny<string>()), Times.Once);
        }
    }
}
