using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Web.Controllers.Search;

namespace Catalogue.Web.Search.Service
{
    public interface ISearchService
    {
        SearchOutputModel FindByKeyword(string query);
        SearchOutputModel Find(string q, int p = 1);
    }

    public class SearchService : ISearchService
    {
        private ISearchRepository _searchRepository;

        public SearchService(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        public SearchOutputModel FindByKeyword(string query)
        {
            return _searchRepository.FindBykeyword(query);
        }
        public SearchOutputModel Find(string q, int p =1)
        {
            return _searchRepository.Find(q, p);
        }
    }
}