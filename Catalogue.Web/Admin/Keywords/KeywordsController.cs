using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Web.Admin.Keywords;

namespace Catalogue.Web.Controllers.Keywords
{
    public class KeywordsController : ApiController
    {
        private readonly IKeywordsService _keywordService;

        public KeywordsController(IKeywordsService service)
        {
            _keywordService = service;
        }

        public ICollection<MetadataKeyword> Get(String q)
        {
            return _keywordService.ReadByValue(q);
        }
    }


}
