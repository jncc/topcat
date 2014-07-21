using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        
        [TestFixtureSetUp]
        public void SetUp()
        {
            _keywordsRepository = new KeywordsRepository(Db);
            _keywordsService = new KeywordsService(_keywordsRepository);
        }

        
        
    }
}
