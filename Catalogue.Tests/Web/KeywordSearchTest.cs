using System;
using Catalogue.Web.Controllers.Search;
using Catalogue.Web.Search;
using Catalogue.Web.Search.Service;
using Moq;
using NUnit.Framework;

namespace Catalogue.Tests.Web
{
    internal class KeywordSearchTest : DatabaseTestFixture
    {
        private const string TestStr = "test";

        private readonly SearchOutputModel _searchOutputModel = new SearchOutputModel()
        {
            Total = 1,
            Speed = 100l
        };

        [Test]
        public void ExpectRepositoryCallInService()
        {
            
            var mock = new Mock<ISearchRepository>();

            mock.Setup(m => m.FindBykeyword(It.Is<String>(s => s.Equals(TestStr)))).Returns(_searchOutputModel);
            var keywordSearchService = new SearchService(mock.Object);
           var searchOutputModel = keywordSearchService.Find(TestStr);
            mock.Verify(m => m.FindBykeyword(It.Is<String>(s => s.Equals(TestStr))), Times.Once);
            Assert.Equals(searchOutputModel, _searchOutputModel);
        }

        [Test]
        public void ExpectServiceCallInController()
        {
            var mock = new Mock<ISearchService>();
            mock.Setup(m => m.Find(It.Is<String>(s => s.Equals(TestStr)), It.Is<int>(p => p.Equals(1)))).Returns(_searchOutputModel);
            var keywordSearchController = new KeywordSearchController(mock.Object);
            var searchOutputModel = keywordSearchController.Get(TestStr);
            mock.Verify(m => m.Find(It.Is<String>(s => s.Equals(TestStr)), It.Is<int>(p => p.Equals(1))), Times.Once);
            Assert.Equals(searchOutputModel, _searchOutputModel);
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