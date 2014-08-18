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
        public void ReadAllKeywords()
        {
            ICollection<Keyword> keywords = _keywordsService.ReadAll();
            List<string> uniqueKeywords = keywords.Select(k => k.Vocab + "::" + k.Value).Distinct().ToList();
            Assert.AreEqual(uniqueKeywords.Count, keywords.Count,
                "The index is not working correctly, should only return unique values");
            Assert.AreEqual(TotalExpectedKeywords, keywords.Count,
                "The number of unique keywords in seed project has changed");
        }

        [Test]
        public void ReadByValue()
        {
            //search term "sh" corrisponds to "SeabedMapStatus", "Show on webGIS" of which ther are 177 instances in test data. 
            //index will return 1 result (grouped)
            ICollection<Keyword> keywords = _keywordsService.ReadByValue(testStringValue);
            Assert.IsFalse(keywords.Any(k => !k.Value.StartsWith(testStringValue)));
            Assert.AreEqual(TotalExpectedKeywordByTestValueSh,keywords.Count, "Incorrect number of startswith keywords returned");
        }
    }
}