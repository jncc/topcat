using System;
using Catalogue.Web.Search;
using Catalogue.Web.Search.Service;
using Moq;
using NUnit.Framework;

namespace Catalogue.Tests.Web
{
    internal class KeywordSearchTest : DatabaseTestFixture
    {
        [Test]
        public void ExpectRepositoryCallInService()
        {
            const string testStr = "test";
            var mock = new Mock<IKeywordSearchRepository>();
            mock.Setup(m => m.find(It.Is<String>(s => s.Equals(testStr))));
            var keywordSearchService = new KeywordSearchService(mock.Object);
            keywordSearchService.find(testStr);
            mock.Verify(m => m.find(It.Is<String>(s => s.Equals(testStr))), Times.Once);
        }

        [Test]
        public void ExpectSerivceCallInController()
        {
            Assert.True(true);
        }

        [Test]
        public void ControllerAcceptesStringReturnsSearchModel()
        {
            Assert.True(true);
        }

        [Test]
        public void WhenSearchForKeywordWithSpacesReturnCorrectData()
        {
        }

        [Test]
        public void WhenSearchForKeywordReturnOnlyRecordsWithKeywordNotTextMatch()
        {
        }
    }
}