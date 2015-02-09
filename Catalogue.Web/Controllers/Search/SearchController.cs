using System;
using System.Web.Http;
using Catalogue.Utilities.Text;
using Catalogue.Web.Search;

namespace Catalogue.Web.Controllers.Search
{
    public class SearchController : ApiController
    {
        private readonly ISearchHelper _searchHelper;

        public SearchController(ISearchHelper searchHelper)
        {
            _searchHelper = searchHelper;
        }

        // GET api/search?q=blah
        //todo should accpet array of q.
        public SearchOutputModel Get(string q, string k, int n = 25, int p = 0)
        {
            //could easily rework this to combine the outputs.
            if (k.IsNotBlank()) return KeywordSearch(k, n, p);

            return FullTextSearch(q, n, p);
        }

        private SearchOutputModel FullTextSearch(string q, int n, int p)
        {
            var searchInputModel = new SearchInputModel()
            {
                Keywords = new [] {String.Empty},
                PageNumber = p,
                Query = q,
                NumberOfRecords = n,
            };
            return _searchHelper.FullTextSearch(searchInputModel);
        }

        private SearchOutputModel KeywordSearch(string k, int n, int p)
        {
            var searchInputModel = new SearchInputModel()
            {
                Query = string.Empty,
                Keywords = new[] {k},
                NumberOfRecords = n,
                PageNumber = p,
            };
            return _searchHelper.KeywordSearch(searchInputModel);
        }


    }
}