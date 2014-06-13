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
        SearchOutputModel FindByKeyword(Keyword searchTerm, int n, int page);
        SearchOutputModel Find(string searchTerm, int n,int page );
    }

    public class SearchService : ISearchService
    {
        private ISearchRepository _searchRepository;

        public SearchService(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        public SearchOutputModel FindByKeyword(Keyword searchTerm, int n =0, int page=1)
        {
            return _searchRepository.FindByKeyword(searchTerm, n, page);
        }
        
        public SearchOutputModel Find(string searchTerm,int n = 0, int page =1)
        {
            return _searchRepository.Find(searchTerm,n , page);
        }

        public SearchOutputModel FindByFullTextAndKeyword(string searchTerm, int n= 0, int page = 1)
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