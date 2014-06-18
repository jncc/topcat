using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catalogue.Gemini.Model;
using Catalogue.Web.Controllers.Search;
using Catalogue.Web.Search;
using Catalogue.Web.Search.Service;
using Moq;
using NUnit.Framework;

namespace Catalogue.Tests.Web.Search
{
    internal class KeywordSearchTest : DatabaseTestFixture
    {/*
        private const string TestStr = "test";
        private readonly static Keyword Keyword = new Keyword("Seabed Habitat Maps", "http://vocab.jncc.gov.uk/jncc-broad-category");
        private SearchRepository _searchRepository;
        private SearchService _searchService;

       
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

        private readonly SearchInputModel _searchInputModel = new SearchInputModel(keyword:Keyword, numberOfRecords:0, pageNumber:1 );

      
        [Test]
        public void ExpectRepositoryCallInService()
        {
            
            var mock = new Mock<ISearchRepository>();

            mock.Setup(m => m.FindByKeyword(It.Is<SearchInputModel>(s => s.Equals(_searchInputModel)))).Returns(_searchOutputModel);
            var keywordSearchService = new SearchService(mock.Object);
            var searchOutputModel = keywordSearchService.FindByKeyword(_searchInputModel);
            mock.Verify(m => m.FindByKeyword(It.Is<SearchInputModel>(s => s.Equals(_searchInputModel))), Times.Once);
            Assert.AreEqual(searchOutputModel, _searchOutputModel);
        }

        [Test]
        public void ExpectServiceCallInController()
        {
            var mock = new Mock<ISearchService>();
            mock.Setup(m => m.FindByKeyword(It.Is<SearchInputModel>(s => s.Equals(_searchInputModel)))).Returns(_searchOutputModel);
            var keywordSearchController = new KeywordSearchController(mock.Object);
            var searchOutputModel = keywordSearchController.Post(_searchInputModel);
            mock.Verify(m => m.FindByKeyword(It.Is<SearchInputModel>(s => s.Equals(_searchInputModel))), Times.Once);
            Assert.AreEqual(searchOutputModel, _searchOutputModel);
        }

        [Test]
        public void WhenSearchForKeywordWithSpacesReturnCorrectData()
        {
            // do not perform a full text search, so should be fewer results
            var results = _searchService.FindByKeyword(_searchInputModel);
            Assert.AreEqual(results.Total, 189);
            
        }


        public void WhenSearchForTextReturnKeywordAlso()
        {
           var results = _searchService.FindByFullTextAndKeyword(keyword);
            Assert.AreEqual(results.Total, 256);
            throw new NotImplementedException();
        }*/
    }
}   