using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Gemini.Model;
using Catalogue.Web.Controllers.Search;

namespace Catalogue.Web.Search.Service
{
    public interface ISearchService
    {
        SearchOutputModel Find(SearchInputModel searchInputModel);
        SearchOutputModel FindByKeyword(SearchInputModel searchInputModel);
        SearchOutputModel FindByVocab(SearchInputModel searchInputModel);
    }

    public class SearchService : ISearchService
    {
        private ISearchRepository _searchRepository;

        public SearchService(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        public SearchOutputModel FindByKeyword(SearchInputModel searchInputModel)
        {
            return _searchRepository.FindByKeyword(searchInputModel);
        }

        public SearchOutputModel FindByVocab(SearchInputModel searchInputModel)
        {
            return _searchRepository.FindByVocab(searchInputModel);
        }

        public SearchOutputModel Find(SearchInputModel searchInputModel)
        {
            return _searchRepository.Find(searchInputModel);
        }
    }
}