﻿using Catalogue.Data.Query;
using NUnit.Framework;
using static Catalogue.Data.Query.SortOptions;

namespace Catalogue.Tests.Web.Search
{
    // no idea what this test is for

    internal class SearchTest : DatabaseTestFixture
    {
        private readonly RecordQueryInputModel _recordQueryInputModel = new RecordQueryInputModel()
        {
            Q = "se", 
            N = 25,
            P = 0
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
            var results = _recordQueryer.Search(_recordQueryInputModel);
            Assert.AreEqual(results.Results.Count, 25);
            var totalReturned = results.Results.Count;
            // loop through each page
            int pages = (results.Total + PageSize - 1) / PageSize;
            for (int i = 1; i <= pages; i++)
            {
                _recordQueryInputModel.P = i;
                results = _recordQueryer.Search(_recordQueryInputModel);
                totalReturned += results.Results.Count;
            }
            Assert.AreEqual(results.Total,totalReturned);
        }


        [Test]
        public void WhenPagingCheckCountIsAsExpectedForSortedResults()
        {
            // do not perform a full text search, so should be fewer results
            var queryInput = new RecordQueryInputModel()
            {
                Q = "se",
                N = 25,
                P = 0,
                O = TitleAZ
            };

            var results = _recordQueryer.Search(queryInput);
            Assert.AreEqual(results.Results.Count, 25);
            var totalReturned = results.Results.Count;
            // loop through each page
            int pages = (results.Total + PageSize - 1) / PageSize;
            for (int i = 1; i <= pages; i++)
            {
                _recordQueryInputModel.P = i;
                results = _recordQueryer.Search(_recordQueryInputModel);
                totalReturned += results.Results.Count;
            }
            Assert.AreEqual(results.Total, totalReturned);
        }
    }
}
