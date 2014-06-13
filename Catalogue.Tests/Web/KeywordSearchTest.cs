using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private const string Keyword = "seabed habitat maps";
        private SearchRepository _searchRepository;
        private SearchService _searchService;

        // this isn't working
        [TestFixtureSetUp]
        public void setUp()
        {
            _searchRepository = new SearchRepository(Db);
            _searchService = new SearchService(_searchRepository);
        }

        private readonly SearchOutputModel _searchOutputModel = new SearchOutputModel()
        {
            Total = 1,
            Speed = 100L
        };
      
        [Test]
        public void ExpectRepositoryCallInService()
        {
            
            var mock = new Mock<ISearchRepository>();

            mock.Setup(m => m.FindByKeyword(It.Is<String>(s => s.Equals(TestStr)),It.Is<int>(s => s.Equals(0)) , It.Is<int>(s => s.Equals(1) ))).Returns(_searchOutputModel);
            var keywordSearchService = new SearchService(mock.Object);
           var searchOutputModel = keywordSearchService.FindByKeyword(TestStr);
           mock.Verify(m => m.FindByKeyword(It.Is<String>(s => s.Equals(TestStr)), It.Is<int>(s => s.Equals(0)), It.Is<int>(s => s.Equals(1))), Times.Once);
            Assert.AreEqual(searchOutputModel, _searchOutputModel);
        }

        [Test]
        public void ExpectServiceCallInController()
        {
            var mock = new Mock<ISearchService>();
            mock.Setup(m => m.Find(It.Is<String>(s => s.Equals(TestStr)), It.Is<int>(s => s.Equals(0)), It.Is<int>(p => p.Equals(1)))).Returns(_searchOutputModel);
            var keywordSearchController = new KeywordSearchController(mock.Object);
            var searchOutputModel = keywordSearchController.Get(TestStr);
            mock.Verify(m => m.Find(It.Is<String>(s => s.Equals(TestStr)), It.Is<int>(s => s.Equals(0)), It.Is<int>(p => p.Equals(1))), Times.Once);
            Assert.AreEqual(searchOutputModel, _searchOutputModel);
        }

        [Test]
        public void WhenSearchForKeywordWithSpacesReturnCorrectData()
        {
            // do not perform a full text search, so should be fewer results
            var results = _searchService.FindByKeyword(Keyword);
            Assert.AreEqual(results.Total, 201);
            
        }

        [Test]
        public void WhenSearchForTextReturnKeywordAlso()
        {
            var results = _searchService.FindByFullTextAndKeyword(Keyword);
            Assert.AreEqual(results.Total, 256);
        }
    }
}   