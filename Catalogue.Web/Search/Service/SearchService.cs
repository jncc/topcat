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
        SearchOutputModel FindByKeyword(SearchInputModel searchInputModel);
        SearchOutputModel Find(SearchInputModel searchInputModel);
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

        public SearchOutputModel Find(SearchInputModel searchInputModel)
        {
            return _searchRepository.Find(searchInputModel);
        }

        public SearchOutputModel FindByFullTextAndKeyword(SearchInputModel searchInputModel)
        {
            throw new NotImplementedException();
            /*var keywordOutputModel = FindByKeyword(searchTerm, n,  page);
            var defaultOutputModel = Find(searchTerm,n, page);
            var unioned = defaultOutputModel.Results.Union(keywordOutputModel.Results);
            defaultOutputModel.Results = unioned.ToList();
            defaultOutputModel.Total = defaultOutputModel.Results.Count;
            return defaultOutputModel;*/

        }
    }
}