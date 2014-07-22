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
        private const int TotalExpectedKeywords = 10;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _keywordsRepository = new KeywordsRepository(Db);
            _keywordsService = new KeywordsService(_keywordsRepository);
        }

        [Test]
        public void ReadAllKeywords()
        {
            List<Keyword> keywords = _keywordsService.Read();

            Assert.AreSame(TotalExpectedKeywords, keywords.Count);
            foreach (var keyword in keywords)
            {
                System.Console.WriteLine(keyword.Value);
            }

        }

    }
}
