using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Web.Controllers.Search;

namespace Catalogue.Web.Search
{
    public interface IKeywordSearchRepository
    {
        SearchOutputModel find(String query);
    }

    public class KeywordSearchRepository :IKeywordSearchRepository
    {
        public SearchOutputModel find(string query)
        {
            throw new NotImplementedException();
        }
    }
}