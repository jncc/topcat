using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Gemini.Model;
using Catalogue.Web.Admin.Keywords;
using NUnit.Framework;

namespace Catalogue.Tests.Web.Admin.Keywords
{
    internal class KeywordsServiceTest : DatabaseTestFixture
    {
        private const int TotalExpectedKeywords = 322;

        private const String testStringValue = "Sh";
        private const int TotalExpectedKeywordByTestValueSh = 1;
        private IKeywordsRepository _keywordsRepository;
        private IKeywordsService _keywordsService;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _keywordsRepository = new KeywordsRepository(Db);
            _keywordsService = new KeywordsService(_keywordsRepository);
        }

        [Test]
        public void ReadByValue()
        {
            //search term "sh" corrisponds to "SeabedMapStatus", "Show on webGIS" of which ther are 177 instances in test data. 
            //index will return 1 result (grouped)
            ICollection<MetadataKeyword> keywords = _keywordsService.ReadByValue(testStringValue);
            Assert.IsFalse(keywords.Any(k => !k.Value.StartsWith(testStringValue)));
            Assert.AreEqual(TotalExpectedKeywordByTestValueSh,keywords.Count, "Incorrect number of startswith keywords returned");
        }
    }
}