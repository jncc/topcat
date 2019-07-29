using System;
using System.Collections.Generic;
using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Gemini.Templates;
using Catalogue.Robot.Publishing;
using Catalogue.Robot.Publishing.Client;
using Catalogue.Robot.Publishing.Data;
using Catalogue.Robot.Publishing.Hub;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    public class data_service_specs
    {
        private Env env;

        [OneTimeSetUp]
        public void Init()
        {
            env = new Env(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "TestResources\\.env.test");
        }

        [Test]
        public void large_file_size_test()
        {
            var ftpClientMock = new Mock<IFtpClient>();
            var fileHelperMock = new Mock<IFileHelper>();
            var smtpClientMock = new Mock<ISmtpClient>();
            var dataUploader = new DataService(env, ftpClientMock.Object, fileHelperMock.Object, smtpClientMock.Object);
            fileHelperMock.Setup(x => x.GetFileSizeInBytes("\\\\JNCC-CORPFILE\\Marine Survey\\path\\to\\uploader\\test.txt")).Returns(1000000001);
            
            Action a = () => dataUploader.UploadDataFile("guid-here", @"X:\path\to\uploader\test.txt");
            a.Should().Throw<InvalidOperationException>()
                .And.Message.Should().Be("File at path \\\\JNCC-CORPFILE\\Marine Survey\\path\\to\\uploader\\test.txt is too large to be uploaded by Topcat - manual upload required");

            ftpClientMock.Verify(x => x.UploadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void normal_file_size_test()
        {
            var ftpClientMock = new Mock<IFtpClient>();
            var fileHelperMock = new Mock<IFileHelper>();
            var smtpClientMock = new Mock<ISmtpClient>();
            var dataUploader = new DataService(env, ftpClientMock.Object, fileHelperMock.Object, smtpClientMock.Object);
            fileHelperMock.Setup(x => x.GetFileSizeInBytes("\\\\JNCC-CORPFILE\\Marine Survey\\path\\to\\uploader\\test.txt")).Returns(100000);

            dataUploader.UploadDataFile("guid-here", @"X:\path\to\uploader\test.txt");

            ftpClientMock.Verify(x => x.UploadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
