using System;
using System.Collections.Generic;
using System.Web.Http;
using Catalogue.Web.Admin.Keywords;
using Catalogue.Web.Admin.Vocabularies;

namespace Catalogue.Web.Controllers.Keywords
{
    public class VocabulariesController : ApiController
    {
        private readonly IVocabulariesService _service;

        public VocabulariesController(IVocabulariesService service)
        {
            _service = service;
        }

        public ICollection<string> Get(String q)
        {
            return _service.Read(q);
        }
    }
}