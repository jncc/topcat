using Catalogue.Data.Query;
using Catalogue.Web.Controllers;
using NUnit.Framework;

namespace Catalogue.Tests.Web.Search
{
    // no idea what this test is for

    internal class SearchTest : DatabaseTestFixture
    {
        private readonly RecordQueryInputModel _recordQueryInputModel = new RecordQueryInputModel()
        {
            Q = "se", 
            N= 25,
            P= 0
        };

        private RecordQueryer _recordQueryer;
        private const int PageSize = 25;

        [TestFixtureSetUp]
        public void setUp()
        {
            _recordQueryer = new RecordQueryer(Db);
        }
        [Test]
        public void WhenPagingCheckCountIsAsExpected()
        {
            // do not perform a full text search, so should be fewer results
            var results = _recordQueryer.SearchQuery(_recordQueryInputModel);
            Assert.AreEqual(results.Results.Count, 25);
            var totalReturned = results.Results.Count;
            // loop through each page
            int pages = (results.Total + PageSize - 1) / PageSize;
            for (int i = 1; i <= pages; i++)
            {
                _recordQueryInputModel.P = i;
                results = _recordQueryer.SearchQuery(_recordQueryInputModel);
                totalReturned += results.Results.Count;
            }
            Assert.AreEqual(results.Total,totalReturned);
        }

    
    }
}
