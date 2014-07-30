using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Admin.Keywords
{
    public class KeywordsController : ApiController
    {
        private readonly IKeywordsService _keywordService;

        public KeywordsController(IKeywordsService service)
        {
            _keywordService = service;
        }

        // GET api/records/57d34691-9064-4c1e-90a7-7b0c112daa8d (get a record)

        public ICollection<Keyword> Get(String value = null, String vocab = null)
        {
            return _keywordService.Read(value, vocab);
        }
    }


}
