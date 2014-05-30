using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Web.Controllers.Search;

namespace Catalogue.Web.Search.Service
{
    public interface IKeywordSearchService
    {
        SearchOutputModel find(String query);
    }

    public class KeywordSearchService : IKeywordSearchService
    {
        private IKeywordSearchRepository _keywordSearchRepository;

        public KeywordSearchService(IKeywordSearchRepository keywordSearchRepository)
        {
            _keywordSearchRepository = keywordSearchRepository;
        }

        public SearchOutputModel find(string query)
        {
            return _keywordSearchRepository.find(query);
        }
    }
}