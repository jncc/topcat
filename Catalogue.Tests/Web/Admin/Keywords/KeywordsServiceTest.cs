using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using Catalogue.Web.Admin.Keywords;
using Catalogue.Web.Search;
using Catalogue.Web.Search.Service;
using NUnit.Framework;

namespace Catalogue.Tests.Web.Admin.Keywords
{
    class KeywordsServiceTest : DatabaseTestFixture
    {
        private IKeywordsRepository _keywordsRepository;
        private IKeywordsService _keywordsService;
        private const int TotalExpectedKeywords = 312;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _keywordsRepository = new KeywordsRepository(Db);
            _keywordsService = new KeywordsService(_keywordsRepository);
        }

        [Test]
        public void ReadAllKeywords()
        {
            ICollection<Keyword> keywords = _keywordsService.Read();
            List<Keyword> keywordList = keywords.ToList();
            keywordList.Sort();
            Assert.AreEqual(TotalExpectedKeywords, keywords.Count);

           /* foreach (var keyword in keywordList)
            {
                System.Console.WriteLine(keyword.Value);
            }
            */
        }

    }
}
